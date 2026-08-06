using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KcetasAboneApi.Models; 
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KcetasAboneApi.Services
{
    /// <summary>
    /// Dış sistemlere (GİB, Banka vb.) gönderilmek üzere Outbox tablosunda bekleyen mesajları periyodik olarak okuyan ve ilgili dış uç noktalara asenkron olarak ileten arka plan servisidir.
    /// </summary>
    public class OutboxWorkerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OutboxWorkerService> _logger;

        public OutboxWorkerService(IServiceProvider serviceProvider, ILogger<OutboxWorkerService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// Arka plan görevinin ana çalışma döngüsüdür. 'BEKLIYOR' statüsündeki mesajları alır, API'ye HTTP POST yapar, sonucu 'GONDERILDI' veya hatalıysa 'MANUEL_MUDAHALE' olarak günceller.
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Outbox Worker entegrasyon servisi başlatıldı.");

            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
                    var client = httpClientFactory.CreateClient();

                    var bekleyenKayitlar = context.EntegrasyonOutboxes
                        .Where(o => o.Durum == OutboxDurumu.BEKLIYOR && o.RetryCount < 3)
                        .Take(100)
                        .ToList();

                    foreach (var kayit in bekleyenKayitlar)
                    {
                        _logger.LogInformation($"[OutboxId: {kayit.OutboxId}] Entegrasyon işlemi başlatıldı. Hedef Sistem: {kayit.HedefSistem}");

                        try
                        {
                            var content = new StringContent(kayit.Payload, Encoding.UTF8, "application/json");

                            var response = await client.PostAsync("http://localhost:5296/api/entegrator/gib-gonder", content);
                            var responseString = await response.Content.ReadAsStringAsync();

                            if (response.IsSuccessStatusCode)
                            {
                                kayit.Durum = OutboxDurumu.GONDERILDI;
                                kayit.GonderimZamani = DateTime.UtcNow;
                                _logger.LogInformation($"[OutboxId: {kayit.OutboxId}] İşlem başarılı. Belge hedefe iletildi.");
                            }
                            else
                            {
                                kayit.RetryCount += 1;
                                kayit.HataKodu = ((int)response.StatusCode).ToString();
                                kayit.HataMesaji = responseString;
                                kayit.SonDenemeTarihi = DateTime.UtcNow;
                                
                                _logger.LogWarning($"[OutboxId: {kayit.OutboxId}] İşlem başarısız. Deneme Sayısı: {kayit.RetryCount}, Hata: {responseString}");

                                if (kayit.RetryCount >= 3)
                                {
                                    kayit.Durum = OutboxDurumu.MANUEL_MUDAHALE;
                                    _logger.LogError($"[OutboxId: {kayit.OutboxId}] Maksimum deneme sayısına (3) ulaşıldı. Kayıt 'MANUEL_MUDAHALE' statüsüne çekildi.");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"[OutboxId: {kayit.OutboxId}] Sunucu bağlantı hatası: {ex.Message}");
                            kayit.RetryCount += 1;
                        }
                    }

                    if (bekleyenKayitlar.Any())
                    {
                        await context.SaveChangesAsync();
                    }
                }

                await Task.Delay(10000, stoppingToken);
            }
        }
    }
}
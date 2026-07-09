using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KcetasAboneApi.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KcetasAboneApi.Services
{
    public class SayacAvcisiWorkerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SayacAvcisiWorkerService> _logger;

        public SayacAvcisiWorkerService(IServiceProvider serviceProvider, ILogger<SayacAvcisiWorkerService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Yaşlı Sayaç Avcısı (Worker Service) başlatıldı. Her 5 dakikada bir kontrol yapacak.");

            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    int currentYear = 2026; // Veya DateTime.UtcNow.Year kullanabiliriz, plana göre 2026 baz alındı.

                    // Aktif olan, bir tüketim noktasına bağlı olan ve 5 yaşından büyük sayaçları bul
                    var yasliSayaclar = context.Sayaclars
                        .Where(s => s.TuketimNoktasiId != null 
                                 && s.Durum != "IPTAL" // İptal edilmemiş
                                 && (currentYear - s.UretimYili) >= 5)
                        .ToList();

                    if (yasliSayaclar.Any())
                    {
                        _logger.LogInformation($"{yasliSayaclar.Count} adet 5 yaşını doldurmuş aktif sayaç bulundu. Değiştirme iş emirleri kontrol ediliyor...");

                        foreach (var sayac in yasliSayaclar)
                        {
                            // Bu tüketim noktası için halihazırda açık bir "DEGISTIRME" iş emri var mı kontrol et
                            bool acikIsEmriVar = context.IsEmirleris
                                .Any(ie => ie.TuketimNoktasiId == sayac.TuketimNoktasiId 
                                        && ie.Tip == "DEGISTIRME" 
                                        && (ie.Durum == "ACIK" || ie.Durum == "ATANDI" || ie.Durum == "SAHADA"));

                            if (!acikIsEmriVar)
                            {
                                // Yoksa yeni iş emri oluştur
                                var yeniIsEmri = new IsEmirleri
                                {
                                    IsEmriNo = "IE-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                                    TuketimNoktasiId = sayac.TuketimNoktasiId.Value,
                                    SayacId = sayac.SayacId,
                                    Tip = "DEGISTIRME",
                                    Oncelik = "YUKSEK",
                                    Durum = "ACIK",
                                    Gerekce = "Sayaç 5 yılını doldurduğu için otomatik değişim talebi.",
                                    CreatedAt = DateTime.UtcNow
                                };

                                context.IsEmirleris.Add(yeniIsEmri);
                                _logger.LogInformation($"Sayaç (ID: {sayac.SayacId}, Üretim: {sayac.UretimYili}) için {yeniIsEmri.IsEmriNo} numaralı değişim iş emri oluşturuldu.");
                            }
                        }

                        // Değişiklikleri veritabanına kaydet
                        await context.SaveChangesAsync(stoppingToken);
                    }
                    else
                    {
                        _logger.LogInformation("Değişmesi gereken yaşlı sayaç bulunamadı.");
                    }
                }

                // Test için 5 dakikada bir çalışacak. (Gerçekte gece 03:00 için Task.Delay hesabı yapılabilir)
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}

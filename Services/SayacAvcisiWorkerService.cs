using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KcetasAboneApi.Models;
using Microsoft.EntityFrameworkCore;
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
                    int currentYear = 2026; 

                    var yasliSayaclar = await context.Sayaclars
                        .Where(s => s.TuketimNoktasiId != null 
                                 && s.Durum != SayacDurumu.IPTAL 
                                 && (currentYear - s.UretimYili) >= 5)
                        .ToListAsync(stoppingToken);

                    if (yasliSayaclar.Any())
                    {
                        _logger.LogInformation($"{yasliSayaclar.Count} adet 5 yaşını doldurmuş aktif sayaç bulundu. Kontrol ediliyor...");

                        // 1. Prefix ve Sıra Numarası Raconu (Döngü Dışında)
                        string prefix = $"IE-{DateTime.UtcNow:yyyyMM}-";

                        // 2. Veritabanındaki en yüksek sırayı al (Sıralama hatasını önlemek için Int dönüşümü burada)
                        var sonSira = await context.IsEmirleris
                            .Where(x => x.IsEmriNo.StartsWith(prefix))
                            .Select(x => x.IsEmriNo.Substring(x.IsEmriNo.Length - 4))
                            .OrderByDescending(x => x)
                            .FirstOrDefaultAsync(stoppingToken);

                        int yeniSira = 1;
                        if (!string.IsNullOrEmpty(sonSira) && int.TryParse(sonSira, out int sonSiraInt))
                        {
                            yeniSira = sonSiraInt + 1;
                        }

                        // 2. İş Emri Kontrolü ve Oluşturma
                        foreach (var sayac in yasliSayaclar)
                        {
                            bool acikIsEmriVar = await context.IsEmirleris
                                .AnyAsync(ie => ie.TuketimNoktasiId == sayac.TuketimNoktasiId 
                                             && ie.Tip == IsEmriTipi.DEGISTIRME 
                                             && (ie.Durum == IsEmriDurumu.ACIK || ie.Durum == IsEmriDurumu.ATANDI || ie.Durum == IsEmriDurumu.SAHADA), stoppingToken);

                            if (!acikIsEmriVar)
                            {
                                var yeniIsEmri = new IsEmirleri
                                {
                                    IsEmriNo = $"{prefix}{yeniSira:D4}",
                                    TuketimNoktasiId = sayac.TuketimNoktasiId.Value,
                                    SayacId = sayac.SayacId,
                                    Tip = IsEmriTipi.DEGISTIRME,
                                    Oncelik = "YUKSEK",
                                    Durum = IsEmriDurumu.ACIK,
                                    Gerekce = "Sayaç 5 yılını doldurduğu için otomatik değişim talebi.",
                                    CreatedAt = DateTime.UtcNow
                                };

                                context.IsEmirleris.Add(yeniIsEmri);
                                _logger.LogInformation($"İş emri oluşturuldu: {yeniIsEmri.IsEmriNo}");
                                
                                yeniSira++; // Numarayı her seferinde artır
                            }
                        }

                        await context.SaveChangesAsync(stoppingToken);
                    }
                    else
                    {
                        _logger.LogInformation("Değişmesi gereken yaşlı sayaç bulunamadı.");
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}
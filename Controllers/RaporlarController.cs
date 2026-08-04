using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KcetasAboneApi.Models;

namespace KcetasAboneApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RaporlarController : ControllerBase
{
    private readonly AppDbContext _context;

    public RaporlarController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("Dashboard")]
    public async Task<IActionResult> GetDashboardRaporu()
    {
        // 1. Aktif Abone ve Tüketim Noktası Sayıları
        var aktifAboneSayisi = await _context.Abonelers.CountAsync(a => a.Status == "AKTIF");
        var aktifTuketimNoktasiSayisi = await _context.TuketimNoktasis.CountAsync(t => t.Status == "AKTIF");

        // 2. Borç Nedeniyle Kesme Adayları
        // Son ödeme tarihi geçmiş ve ödenmemiş/bekleyen faturalar
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var kesmeAdayiFaturaSayisi = await _context.Faturas
            .CountAsync(f => 
                f.SonOdemeTarihi < today && 
                (f.Durum == FaturaDurumu.ONAYLANDI || f.Durum == FaturaDurumu.GONDERILDI || f.Durum == FaturaDurumu.ODENMEDI));

        // 3. Dönemsel Tahakkuk (Son 6 ay veya sistemdeki döneme göre gruplama)
        var tahakkukGrafikVerisi = await _context.Faturas
            .Where(f => f.Durum == FaturaDurumu.ONAYLANDI || f.Durum == FaturaDurumu.GONDERILDI || f.Durum == FaturaDurumu.ODENDI || f.Durum == FaturaDurumu.ODENMEDI)
            .GroupBy(f => f.Donem)
            .Select(g => new 
            {
                Donem = g.Key,
                ToplamTuketimKwh = g.Sum(x => x.TuketimKwh),
                ToplamTahakkuk = g.Sum(x => x.ToplamTutar)
            })
            .OrderByDescending(x => x.Donem)
            .Take(6)
            .ToListAsync();

        // 4. Entegrasyon Sağlık Raporu
        var entegrasyonIstatistikleri = await _context.EntegrasyonOutboxes
            .GroupBy(e => e.Durum)
            .Select(g => new 
            {
                Durum = g.Key.ToString(),
                Sayi = g.Count()
            })
            .ToListAsync();

        // 5. Açık İş Emirleri
        var acikIsEmriSayisi = await _context.IsEmirleris
            .CountAsync(i => i.Durum == IsEmriDurumu.ACIK || i.Durum == IsEmriDurumu.ATANDI || i.Durum == IsEmriDurumu.SAHADA);

        return Ok(new
        {
            Ozet = new 
            {
                AktifAboneSayisi = aktifAboneSayisi,
                AktifTuketimNoktasiSayisi = aktifTuketimNoktasiSayisi,
                KesmeAdayiFaturaSayisi = kesmeAdayiFaturaSayisi,
                AcikIsEmriSayisi = acikIsEmriSayisi
            },
            Tahakkuk = tahakkukGrafikVerisi.OrderBy(x => x.Donem), // Tarih sırasına göre artan (eskiden yeniye) grafikte daha mantıklıdır
            Entegrasyon = entegrasyonIstatistikleri
        });
    }
}

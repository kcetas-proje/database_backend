using Microsoft.EntityFrameworkCore;
using KcetasAboneApi.Models;

namespace KcetasAboneApi.Services;

public class IsEmiriService : IIsEmiriService
{
    private readonly AppDbContext _context;

    public IsEmiriService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Açık durumdaki bir iş emrini, depodaki belirtilen bir sayaçla eşleştirerek tamamlar. Sayacı ilgili tüketim noktasına takar.
    /// </summary>
    public async Task<(bool Success, string Message, int ProcessedCount)> IsEmriTamamlaAsync(long isEmriId, long sayacId)
    {
        var isEmri = await _context.IsEmirleris.FindAsync(isEmriId);
        if (isEmri == null || isEmri.Durum != IsEmriDurumu.ACIK)
            return (false, "Böyle bir açık iş emri yok.", 0);

        var sayac = await _context.Sayaclars.FindAsync(sayacId);
        if (sayac == null || sayac.Durum != SayacDurumu.DEPODA)
            return (false, "Bu sayaç depoda değil.", 0);

        isEmri.SayacId = sayac.SayacId; 
        isEmri.Durum = IsEmriDurumu.TAMAMLANDI;
        isEmri.UpdatedAt = DateTime.UtcNow; 

        sayac.TuketimNoktasiId = isEmri.TuketimNoktasiId;
        sayac.Durum = SayacDurumu.TAKILI;
        sayac.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return (true, $"İş emri {isEmri.IsEmriNo} başarıyla tamamlandı ve sayaç {sayac.SeriNo} takıldı!", 1);
    }

    /// <summary>
    /// AÇIK durumundaki YENİ_BAĞLANTI iş emirleriyle, DEPODA olan sayaçları eşleştirerek toplu bir şekilde bağlantı işlemini tamamlar.
    /// </summary>
    public async Task<(bool Success, string Message, int ProcessedCount)> TopluYeniBaglantiOnaylaAsync()
    {
        var acikIsEmirleri = await _context.IsEmirleris
            .Where(i => i.Tip == IsEmriTipi.YENI_BAGLANTI && i.Durum == IsEmriDurumu.ACIK)
            .ToListAsync();

        var depodakiSayaclar = await _context.Sayaclars
            .Where(s => s.Durum == SayacDurumu.DEPODA)
            .ToListAsync();

        int islemKapasitesi = Math.Min(acikIsEmirleri.Count, depodakiSayaclar.Count);

        if (islemKapasitesi == 0)
            return (false, "Sahada açık iş emri yok ya da depoda sayaç kalmamış!", 0);

        for (int i = 0; i < islemKapasitesi; i++)
        {
            var isEmri = acikIsEmirleri[i];
            var sayac = depodakiSayaclar[i];

            isEmri.SayacId = sayac.SayacId; 
            isEmri.Durum = IsEmriDurumu.TAMAMLANDI;

            sayac.TuketimNoktasiId = isEmri.TuketimNoktasiId;
            sayac.Durum = SayacDurumu.TAKILI;
        }

        await _context.SaveChangesAsync();

        return (true, $"{islemKapasitesi} adet YENI_BAGLANTI iş emri başarıyla tamamlandı ve sayaçlar mekanlara takıldı!", islemKapasitesi);
    }
}

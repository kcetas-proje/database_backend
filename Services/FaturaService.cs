using Microsoft.EntityFrameworkCore;
using KcetasAboneApi.Models;

namespace KcetasAboneApi.Services;

public class FaturaService : IFaturaService
{
    private readonly AppDbContext _context;

    public FaturaService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(int FaturaSayisi, decimal ToplamTutar)> OnaylanmisEndeksleriFaturalandirAsync()
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var okunmusEndeksler = await _context.EndeksOkumas
                .Include(e => e.Sozlesme).ThenInclude(s => s!.TuketimNoktasi) 
                .Include(e => e.Sozlesme).ThenInclude(s => s!.Tarife)
                .Include(e => e.Sayac)
                .Where(e => e.DogrulamaDurumu == DogrulamaDurumu.ONAYLANDI)
                .ToListAsync();

            if (!okunmusEndeksler.Any())
                return (0, 0m);

            var yeniFaturalar = new List<Fatura>();
            string faturaPrefix = $"FAT-{DateTime.UtcNow:yyyyMM}-";
            
            int faturaSira = await _context.Faturas.CountAsync(f => f.FaturaTarihi.Year == DateTime.UtcNow.Year && f.FaturaTarihi.Month == DateTime.UtcNow.Month) + 1;

            foreach (var endeks in okunmusEndeksler)
            {
                decimal hamTuketim = (decimal)(endeks.YeniEndeks - endeks.OncekiEndeks!);
                if (hamTuketim <= 0) continue; 

                decimal carpan = endeks.Sayac?.Carpan ?? 1m;
                decimal gercekTuketimKwh = hamTuketim * carpan;

                var tarife = endeks.Sozlesme!.Tarife;

                decimal enerjiBedeli = gercekTuketimKwh * tarife!.GunduzBirimFiyat; 
                decimal dagitimBedeli = gercekTuketimKwh * tarife.DagitimBedeli;
                decimal hizmetBedeli = 15.50m; 

                decimal vergisizToplam = enerjiBedeli + dagitimBedeli;
                decimal vergiFon = vergisizToplam * (tarife.KdvOrani / 100m); 
                decimal toplamTutar = vergisizToplam + hizmetBedeli + vergiFon;

                var yeniFatura = new Fatura
                {
                    FaturaNo = $"{faturaPrefix}{faturaSira:D5}",
                    SozlesmeId = endeks.SozlesmeId ?? 0,
                    TekilKod = endeks.Sozlesme!.TuketimNoktasi!.TekilKod, 
                    FaturaTipi = FaturaTipi.DONEM,
                    Donem = endeks.Donem ?? string.Empty,
                    FaturaTarihi = DateOnly.FromDateTime(DateTime.UtcNow),
                    SonOdemeTarihi = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                    OkumaId = endeks.OkumaId,
                    IlkEndeks = endeks.OncekiEndeks,
                    SonEndeks = endeks.YeniEndeks,
                    TuketimKwh = Math.Round(gercekTuketimKwh, 2),
                    ReaktifEnduktif = 0m,
                    ReaktifKapasitif = 0m,
                    Carpan = carpan,
                    EnerjiBedeli = Math.Round(enerjiBedeli, 2),
                    DagitimBedeli = Math.Round(dagitimBedeli, 2),
                    HizmetBedeli = hizmetBedeli,
                    KesmeBaglamaBedeli = 0m, 
                    VergiFonToplam = Math.Round(vergiFon, 2),
                    ToplamTutar = Math.Round(toplamTutar, 2),
                    Durum = FaturaDurumu.HESAPLANDI,
                    Status = "AKTIF",
                    CreatedAt = DateTime.UtcNow,

                    FaturaKalemis = new List<FaturaKalemi>
                    {
                        new FaturaKalemi { KalemTipi = KalemTipi.ENERJI, Miktar = gercekTuketimKwh, BirimFiyat = tarife.GunduzBirimFiyat, Tutar = Math.Round(enerjiBedeli, 2), Aciklama = "Aktif Enerji Bedeli" },
                        new FaturaKalemi { KalemTipi = KalemTipi.DAGITIM, Miktar = gercekTuketimKwh, BirimFiyat = tarife.DagitimBedeli, Tutar = Math.Round(dagitimBedeli, 2), Aciklama = "Dağıtım Sistemi Kullanım Bedeli" },
                        new FaturaKalemi { KalemTipi = KalemTipi.HIZMET, Miktar = 1, BirimFiyat = hizmetBedeli, Tutar = hizmetBedeli, Aciklama = "Sabit Hizmet Bedeli" },
                        new FaturaKalemi { KalemTipi = KalemTipi.VERGI_FON, Miktar = 1, BirimFiyat = Math.Round(vergiFon, 2), Tutar = Math.Round(vergiFon, 2), Aciklama = "KDV ve Diğer Fonlar" }
                    }
                };

                yeniFaturalar.Add(yeniFatura);
                endeks.DogrulamaDurumu = DogrulamaDurumu.TAHAKKUKA_AKTARILDI;
                faturaSira++;
            }

            _context.Faturas.AddRange(yeniFaturalar);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (yeniFaturalar.Count, yeniFaturalar.Sum(f => f.ToplamTutar));
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}

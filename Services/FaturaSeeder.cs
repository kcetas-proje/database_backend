using KcetasAboneApi.Models;
using KcetasSeeder.Helpers;
using Microsoft.EntityFrameworkCore;

namespace KcetasAboneApi.Services;

public class FaturaSeeder
{
    private readonly AppDbContext _context;

    public FaturaSeeder(AppDbContext context)
    {
        _context = context;
    }

    public async Task Generate()
    {
        const int batchSize = 1000;

        _context.ChangeTracker.AutoDetectChangesEnabled = false;

        var mevcutOkumalar = await _context.Faturas
            .AsNoTracking()
            .Where(x => x.OkumaId != null)
            .Select(x => x.OkumaId!.Value)
            .ToHashSetAsync();

        var okumalar = await _context.EndeksOkumas
            .AsNoTracking()
            .Include(x => x.Sozlesme)
                .ThenInclude(x => x!.Tarife)
            .Include(x => x.Sozlesme)
                .ThenInclude(x => x!.TuketimNoktasi)
            .Where(x =>
                x.Sozlesme != null &&
                x.Sozlesme.Tarife != null &&
                x.Sozlesme.TuketimNoktasi != null &&
                !mevcutOkumalar.Contains(x.OkumaId))
            .OrderBy(x => x.OkumaId)
            .ToListAsync();

        if (!okumalar.Any())
        {
            Console.WriteLine("Faturalandırılacak kayıt bulunamadı.");
            return;
        }

        int sonFaturaNo =
            await NumaraGenerator.GetLastFaturaNumber(_context);

        var faturalar = new List<Fatura>();
        var kalemler = new List<FaturaKalemi>();

        int olusturulan = 0;

        foreach (var okuma in okumalar)
        {
            sonFaturaNo++;

            decimal tuketim =
                okuma.YeniEndeks - (okuma.OncekiEndeks ?? 0);

            var tarife = okuma.Sozlesme!.Tarife!;

            decimal enerjiBedeli =
                Math.Round(tuketim * tarife.GunduzBirimFiyat, 2);

            decimal dagitimBedeli =
                Math.Round(tuketim * tarife.DagitimBedeli, 2);

            decimal hizmetBedeli = 25m;

            decimal kesmeBaglamaBedeli = 0m;

            decimal araToplam =
                enerjiBedeli +
                dagitimBedeli +
                hizmetBedeli +
                kesmeBaglamaBedeli;

            decimal vergi =
                Math.Round(araToplam * tarife.KdvOrani / 100, 2);

            decimal toplam = araToplam + vergi;

            var fatura = new Fatura
            {
                FaturaNo = $"FTR-38-{sonFaturaNo:D6}",

                SozlesmeId = okuma.SozlesmeId!.Value,

                TekilKod = okuma.Sozlesme.TuketimNoktasi!.TekilKod,

                FaturaTipi = "DONEM",

                Donem = okuma.Donem!,

                FaturaTarihi =
                    DateOnly.FromDateTime(DateTime.UtcNow.Date),

                SonOdemeTarihi =
                    DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(15)),

                OkumaId = okuma.OkumaId,

                IlkEndeks = okuma.OncekiEndeks,

                SonEndeks = okuma.YeniEndeks,

                TuketimKwh = tuketim,

                ReaktifEnduktif = 0,

                ReaktifKapasitif = 0,

                Carpan = 1,

                EnerjiBedeli = enerjiBedeli,

                DagitimBedeli = dagitimBedeli,

                HizmetBedeli = hizmetBedeli,

                KesmeBaglamaBedeli = kesmeBaglamaBedeli,

                VergiFonToplam = vergi,

                ToplamTutar = toplam,

                Durum = FaturaDurumu.HESAPLANDI,

                Status = "AKTIF",

                CreatedAt = DateTime.UtcNow
            };

            faturalar.Add(fatura);

            if (faturalar.Count >= batchSize)
            {
                await _context.Faturas.AddRangeAsync(faturalar);
                await _context.SaveChangesAsync();

                foreach (var f in faturalar)
                {
                    kalemler.Add(new FaturaKalemi
                    {
                        FaturaId = f.FaturaId,
                        KalemTipi = "ENERJI",
                        Aciklama = "Enerji Bedeli",
                        Miktar = f.TuketimKwh ?? 0m,
                        BirimFiyat = (f.TuketimKwh ?? 0m) == 0
        ? 0m
        : Math.Round(f.EnerjiBedeli / (f.TuketimKwh ?? 1m), 4),
                        Tutar = f.EnerjiBedeli,
                        CreatedAt = DateTime.UtcNow
                    });

                    kalemler.Add(new FaturaKalemi
                    {
                        FaturaId = f.FaturaId,
                        KalemTipi = "DAGITIM",
                        Aciklama = "Dağıtım Bedeli",
                        Miktar = f.TuketimKwh ?? 0m,
                        BirimFiyat = (f.TuketimKwh ?? 0m) == 0
                            ? 0m
                            : Math.Round(f.DagitimBedeli / (f.TuketimKwh ?? 1m), 4),
                        Tutar = f.DagitimBedeli,
                        CreatedAt = DateTime.UtcNow
                    });

                    kalemler.Add(new FaturaKalemi
                    {
                        FaturaId = f.FaturaId,
                        KalemTipi = "HIZMET",
                        Aciklama = "Hizmet Bedeli",
                        Miktar = 1m,
                        BirimFiyat = f.HizmetBedeli,
                        Tutar = f.HizmetBedeli,
                        CreatedAt = DateTime.UtcNow
                    });

                    kalemler.Add(new FaturaKalemi
                    {
                        FaturaId = f.FaturaId,
                        KalemTipi = "VERGI_FON",
                        Aciklama = "Vergi ve Fonlar",
                        Miktar = 1m,
                        BirimFiyat = f.VergiFonToplam,
                        Tutar = f.VergiFonToplam,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                await _context.FaturaKalemis.AddRangeAsync(kalemler);
                await _context.SaveChangesAsync();

                olusturulan += faturalar.Count;

                Console.WriteLine($"{olusturulan:N0} fatura oluşturuldu...");

                _context.ChangeTracker.Clear();

                faturalar.Clear();
                kalemler.Clear();
            }
        }

        if (faturalar.Any())
        {
            await _context.Faturas.AddRangeAsync(faturalar);
            await _context.SaveChangesAsync();

            foreach (var f in faturalar)
            {
                kalemler.Add(new FaturaKalemi
                {
                    FaturaId = f.FaturaId,
                    KalemTipi = "ENERJI",
                    Aciklama = "Enerji Bedeli",
                    Miktar = f.TuketimKwh ?? 0m,

                    BirimFiyat = (f.TuketimKwh ?? 0m) == 0m
    ? 0m
    : Math.Round(
        f.EnerjiBedeli / (f.TuketimKwh ?? 1m),
        4),
                    Tutar = f.EnerjiBedeli,
                    CreatedAt = DateTime.UtcNow
                });

                kalemler.Add(new FaturaKalemi
                {
                    FaturaId = f.FaturaId,
                    KalemTipi = "DAGITIM",
                    Aciklama = "Dağıtım Bedeli",
                    Miktar = f.TuketimKwh ?? 0m,

                    BirimFiyat = (f.TuketimKwh ?? 0m) == 0m
    ? 0m
    : Math.Round(
        f.DagitimBedeli / (f.TuketimKwh ?? 1m),
        4),
                    Tutar = f.DagitimBedeli,
                    CreatedAt = DateTime.UtcNow
                });

                kalemler.Add(new FaturaKalemi
                {
                    FaturaId = f.FaturaId,
                    KalemTipi = "HIZMET",
                    Aciklama = "Hizmet Bedeli",
                    Miktar = 1,
                    BirimFiyat = f.HizmetBedeli,
                    Tutar = f.HizmetBedeli,
                    CreatedAt = DateTime.UtcNow
                });

                kalemler.Add(new FaturaKalemi
                {
                    FaturaId = f.FaturaId,
                    KalemTipi = "VERGI_FON",
                    Aciklama = "Vergi ve Fonlar",
                    Miktar = 1m,
                    BirimFiyat = f.VergiFonToplam,
                    Tutar = f.VergiFonToplam,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.FaturaKalemis.AddRangeAsync(kalemler);
            await _context.SaveChangesAsync();

            olusturulan += faturalar.Count;

            _context.ChangeTracker.Clear();
        }

        _context.ChangeTracker.AutoDetectChangesEnabled = true;

        Console.WriteLine();
        Console.WriteLine($"Toplam {olusturulan:N0} adet fatura oluşturuldu.");
    }
}
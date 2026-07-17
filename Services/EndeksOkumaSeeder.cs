using Bogus;
using KcetasAboneApi.Models;
using KcetasSeeder.Helpers;
using Microsoft.EntityFrameworkCore;

namespace KcetasAboneApi.Services;

public class EndeksOkumaSeeder
{
    private readonly AppDbContext _context;

    public EndeksOkumaSeeder(AppDbContext context)
    {
        _context = context;
    }

    public async Task Generate(int aySayisi)
    {
        const int batchSize = 1000;

        _context.ChangeTracker.AutoDetectChangesEnabled = false;

        var mevcutSozlesmeler = await _context.EndeksOkumas
            .AsNoTracking()
            .Select(x => x.SozlesmeId)
            .Distinct()
            .ToHashSetAsync();

        var kaynaklar = await (
            from sozlesme in _context.Sozlesmelers.AsNoTracking()
            join sayac in _context.Sayaclars.AsNoTracking()
                on sozlesme.TuketimNoktasiId equals sayac.TuketimNoktasiId
            where sozlesme.Durum == "AKTIF"
                  && !mevcutSozlesmeler.Contains(sozlesme.SozlesmeId)
            select new
            {
                sozlesme.SozlesmeId,
                sayac.SayacId
            })
            .ToListAsync();

        if (!kaynaklar.Any())
        {
            Console.WriteLine("Endeks oluşturulacak uygun kayıt bulunamadı.");
            return;
        }

        var faker = new Faker("tr");

        var liste = new List<EndeksOkuma>();

        foreach (var kaynak in kaynaklar)
        {
            decimal mevcutEndeks = faker.Random.Decimal(1000, 5000);

            for (int ay = aySayisi - 1; ay >= 0; ay--)
            {
                var okumaTarihi = DateTime.UtcNow.Date.AddMonths(-ay);

                decimal tuketim = FakerHelper.Tuketim();

                decimal yeniEndeks = mevcutEndeks + tuketim;

                liste.Add(new EndeksOkuma
                {
                    SayacId = kaynak.SayacId,

                    SozlesmeId = kaynak.SozlesmeId,

                    IsEmriId = null,

                    OkumaTipi = "RUTIN_DONEM",

                    OkumaKaynagi = "MANUEL",

                    OncekiEndeks = mevcutEndeks,

                    YeniEndeks = yeniEndeks,

                    Donem = okumaTarihi.ToString("yyyy-MM"),

                    OkumaZamani = okumaTarihi,

                    KullaniciId = null,

                    OkunamamaNedeni = null,

                    DogrulamaDurumu = "ONAYLANDI",

                    AnomaliMi = false,

                    Status = "AKTIF",

                    CreatedAt = DateTime.UtcNow
                });

                mevcutEndeks = yeniEndeks;

                if (liste.Count >= batchSize)
                {
                    await _context.EndeksOkumas.AddRangeAsync(liste);
                    await _context.SaveChangesAsync();

                    _context.ChangeTracker.Clear();
                    liste.Clear();
                }
            }
        }

        if (liste.Any())
        {
            await _context.EndeksOkumas.AddRangeAsync(liste);
            await _context.SaveChangesAsync();

            _context.ChangeTracker.Clear();
        }

        _context.ChangeTracker.AutoDetectChangesEnabled = true;

        Console.WriteLine($"{kaynaklar.Count * aySayisi} adet endeks okuması oluşturuldu.");
    }
}
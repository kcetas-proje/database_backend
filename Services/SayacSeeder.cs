using Bogus;
using KcetasAboneApi.Models;
using KcetasSeeder.Helpers;
using Microsoft.EntityFrameworkCore;

namespace KcetasAboneApi.Services;

public class SayacSeeder
{
    private readonly AppDbContext _context;

    public SayacSeeder(AppDbContext context)
    {
        _context = context;
    }

    public async Task Generate()
    {
        const int batchSize = 1000;

        _context.ChangeTracker.AutoDetectChangesEnabled = false;

        var tuketimNoktalari = await _context.TuketimNoktasis
            .Include(x => x.Sayaclars)
            .ToListAsync();

        if (!tuketimNoktalari.Any())
        {
            Console.WriteLine("Önce tüketim noktaları oluşturulmalıdır.");
            return;
        }

        int sonSeriNo =
            await NumaraGenerator.GetLastSayacNumber(_context);

        var faker = new Faker("tr");

        string[] markalar =
        {
            "Luna",
            "Siemens",
            "ABB",
            "Schneider",
            "Kael",
            "Landis+Gyr"
        };

        string[] modeller =
        {
            "E100",
            "E200",
            "MX300",
            "A500",
            "DTS541"
        };

        var liste = new List<Sayaclar>();

        int olusturulan = 0;

        foreach (var nokta in tuketimNoktalari)
        {
            if (nokta.Sayaclars.Any())
                continue;

            sonSeriNo++;

            var durum = faker.PickRandom(
                "TAKILI",
                "TAKILI",
                "TAKILI",
                "TAKILI",
                "TAKILI",
                "DEPODA",
                "ARIZALI");

            liste.Add(new Sayaclar
            {
                SeriNo = sonSeriNo.ToString(),

                TuketimNoktasiId = durum == "TAKILI"
                    ? nokta.TuketimNoktasiId
                    : null,

                Marka = faker.PickRandom(markalar),

                Model = faker.PickRandom(modeller),

                UretimYili = faker.Random.Int(2018, DateTime.Now.Year),

                Faz = faker.Random.Bool(0.75f)
                    ? Faz.TEK_FAZ
                    : Faz.UC_FAZ,

                Carpan = faker.PickRandom(new decimal[]
                {
            1,
            5,
            10
                }),

                MuhurNo = $"MHR-{faker.Random.Number(100000, 999999)}",

                Durum = durum switch
                {
                    "TAKILI" => SayacDurumu.TAKILI,
                    "DEPODA" => SayacDurumu.DEPODA,
                    _ => SayacDurumu.ARIZALI
                },

                CreatedAt = DateTime.UtcNow,

                CreatedBy = null,

                UpdatedAt = null,

                UpdatedBy = null
            });

            if (liste.Count >= batchSize)
            {
                await _context.Sayaclars.AddRangeAsync(liste);
                await _context.SaveChangesAsync();

                olusturulan += liste.Count;

                _context.ChangeTracker.Clear();
                liste.Clear();
            }
        }

        if (liste.Any())
        {
            await _context.Sayaclars.AddRangeAsync(liste);
            await _context.SaveChangesAsync();

            olusturulan += liste.Count;

            _context.ChangeTracker.Clear();
        }

        _context.ChangeTracker.AutoDetectChangesEnabled = true;

        Console.WriteLine($"{olusturulan} adet sayaç oluşturuldu.");
    }
}

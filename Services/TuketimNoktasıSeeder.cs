using KcetasAboneApi.Models;
using KcetasSeeder.Helpers;
using Microsoft.EntityFrameworkCore;

namespace KcetasAboneApi.Services;

public class TuketimNoktasiSeeder
{
    private readonly AppDbContext _context;

    public TuketimNoktasiSeeder(AppDbContext context)
    {
        _context = context;
    }

    public async Task Generate(int adet)
    {
        const int batchSize = 1000;

        _context.ChangeTracker.AutoDetectChangesEnabled = false;

        var ilceler = await _context.Ilces.ToListAsync();

        if (!ilceler.Any())
        {
            Console.WriteLine("İlçe bulunamadı.");
            return;
        }

        int sonTekilKod =
            await NumaraGenerator.GetLastNumber(_context);

        var liste = new List<TuketimNoktasi>();

        for (int i = 0; i < adet; i++)
        {
            sonTekilKod++;

            var ilce = ilceler[Random.Shared.Next(ilceler.Count)];

            liste.Add(new TuketimNoktasi
            {
                TekilKod = $"TK-{DateTime.UtcNow:yyyyMM}-{sonTekilKod:D4}",

                IlceId = ilce.IlceId,

                Mahalle = FakerHelper.Mahalle(),

                BinaNo = FakerHelper.BinaNo(),

                BagimsizBolumNo = FakerHelper.DaireNo(),

                AcikAdres = FakerHelper.AcikAdres(),

                KoordinatLat = FakerHelper.Latitude(),

                KoordinatLon = FakerHelper.Longitude(),

                BaglantiGucuKw = FakerHelper.BaglantiGucu(),

                TuketiciGrubu = FakerHelper.Bool()
                    ? "MESKEN"
                    : "TICARETHANE",

                BaglantiDurumu = BaglantiDurumu.AKTIF,

                Status = "AKTIF",

                CreatedAt = DateTime.UtcNow
            });

            if (liste.Count >= batchSize)
            {
                await _context.TuketimNoktasis.AddRangeAsync(liste);
                await _context.SaveChangesAsync();

                _context.ChangeTracker.Clear();
                liste.Clear();
            }
        }

        if (liste.Any())
        {
            await _context.TuketimNoktasis.AddRangeAsync(liste);
            await _context.SaveChangesAsync();

            _context.ChangeTracker.Clear();
        }

        _context.ChangeTracker.AutoDetectChangesEnabled = true;

        Console.WriteLine($"{adet} adet tüketim noktası oluşturuldu.");
    }
}
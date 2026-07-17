using KcetasAboneApi.Models;
using KcetasSeeder.Helpers;
using Microsoft.EntityFrameworkCore;

namespace KcetasAboneApi.Services;

public class SozlesmeSeeder
{
    private readonly AppDbContext _context;

    public SozlesmeSeeder(AppDbContext context)
    {
        _context = context;
    }

    public async Task Generate(int adet)
    {
        const int batchSize = 1000;

        _context.ChangeTracker.AutoDetectChangesEnabled = false;

        var aboneler = await _context.Abonelers.ToListAsync();

        var kullanilanNoktaIdleri = await _context.Sozlesmelers
            .Select(x => x.TuketimNoktasiId)
            .ToHashSetAsync();

        var tuketimNoktalari = await _context.TuketimNoktasis
            .Where(x => !kullanilanNoktaIdleri.Contains(x.TuketimNoktasiId))
            .ToListAsync();

        var tarifeler = await _context.Tarifelers.ToListAsync();

        if (!aboneler.Any())
        {
            Console.WriteLine("Abone bulunamadı.");
            return;
        }

        if (!tuketimNoktalari.Any())
        {
            Console.WriteLine("Boş tüketim noktası bulunamadı.");
            return;
        }

        if (!tarifeler.Any())
        {
            Console.WriteLine("Tarife bulunamadı.");
            return;
        }

        if (adet > tuketimNoktalari.Count)
        {
            Console.WriteLine(
                $"En fazla {tuketimNoktalari.Count} sözleşme oluşturabilirsiniz.");
            return;
        }

        int sonSozlesmeNo =
            await NumaraGenerator.GetLastSozlesmeNumber(_context);

        var liste = new List<Sozlesmeler>();

        for (int i = 0; i < adet; i++)
        {
            sonSozlesmeNo++;

            var abone = aboneler[Random.Shared.Next(aboneler.Count)];
            var tuketim = tuketimNoktalari[i];
            var tarife = tarifeler[Random.Shared.Next(tarifeler.Count)];

            liste.Add(new Sozlesmeler
            {
                SozlesmeNo = $"SOZ-38-{sonSozlesmeNo:D6}",

                AboneId = abone.AboneId,

                TuketimNoktasiId = tuketim.TuketimNoktasiId,

                TarifeId = tarife.TarifeId,

                SozlesmeTipi = FakerHelper.Bool()
                    ? "BIREYSEL"
                    : "KURUMSAL",

                Durum = "AKTIF",

                BaslangicTarihi =
                    DateOnly.FromDateTime(FakerHelper.GecmisTarih()),

                BitisTarihi = null,

                GuvenceBedeli = Math.Round(
                    tuketim.BaglantiGucuKw *
                    Random.Shared.Next(250, 351), 2),

                CreatedAt = DateTime.UtcNow
            });

            if (liste.Count >= batchSize)
            {
                await _context.Sozlesmelers.AddRangeAsync(liste);
                await _context.SaveChangesAsync();

                _context.ChangeTracker.Clear();
                liste.Clear();
            }
        }

        if (liste.Any())
        {
            await _context.Sozlesmelers.AddRangeAsync(liste);
            await _context.SaveChangesAsync();

            _context.ChangeTracker.Clear();
        }

        _context.ChangeTracker.AutoDetectChangesEnabled = true;

        Console.WriteLine($"{adet} adet sözleşme oluşturuldu.");
    }
}
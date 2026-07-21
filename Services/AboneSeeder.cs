using Bogus;
using KcetasAboneApi.Models;
using KcetasSeeder.Helpers;
using Microsoft.EntityFrameworkCore;

namespace KcetasAboneApi.Services;

public class AboneSeeder
{
    private readonly AppDbContext _context;

    public AboneSeeder(AppDbContext context)
    {
        _context = context;
    }

    public async Task Generate(int adet)
    {
        const int batchSize = 1000;

        _context.ChangeTracker.AutoDetectChangesEnabled = false;

        var faker = new Faker("tr");

        int sonAboneNo =
            await NumaraGenerator.GetLastAboneNumber(_context);

        var kullanilanTckn = await _context.Abonelers
            .Where(x => x.Tckn != null)
            .Select(x => x.Tckn!)
            .ToHashSetAsync();

        var kullanilanVkn = await _context.Abonelers
            .Where(x => x.Vkn != null)
            .Select(x => x.Vkn!)
            .ToHashSetAsync();

        var aboneler = new List<Aboneler>();

        for (int i = 0; i < adet; i++)
        {
            sonAboneNo++;

            bool bireysel = FakerHelper.Bool();

            aboneler.Add(new Aboneler
            {
                AboneNo = $"ABN{DateTime.Now.Year}{sonAboneNo:D6}",

                AboneTipi = bireysel ? AboneTipi.BIREYSEL : AboneTipi.KURUMSAL,

                Ad = bireysel ? FakerHelper.Ad() : null,

                Soyad = bireysel ? FakerHelper.Soyad() : null,

                Unvan = bireysel
                    ? null
                    : faker.Company.CompanyName(),

                Tckn = bireysel
                    ? UniqueTckn(kullanilanTckn)
                    : null,

                Vkn = bireysel
                    ? null
                    : UniqueVkn(kullanilanVkn),

                Telefon = FakerHelper.Telefon(),

                EPosta = FakerHelper.Email(),

                CreatedAt = DateTime.UtcNow
            });

            if (aboneler.Count >= batchSize)
            {
                await _context.Abonelers.AddRangeAsync(aboneler);
                await _context.SaveChangesAsync();

                _context.ChangeTracker.Clear();
                aboneler.Clear();
            }
        }

        if (aboneler.Any())
        {
            await _context.Abonelers.AddRangeAsync(aboneler);
            await _context.SaveChangesAsync();

            _context.ChangeTracker.Clear();
        }

        _context.ChangeTracker.AutoDetectChangesEnabled = true;

        Console.WriteLine($"{adet} adet abone oluşturuldu.");
    }

    private static string UniqueTckn(HashSet<string> kullanilanTckn)
    {
        string tckn;

        do
        {
            tckn = FakerHelper.Tckn();
        }
        while (!kullanilanTckn.Add(tckn));

        return tckn;
    }

    private static string UniqueVkn(HashSet<string> kullanilanVkn)
    {
        string vkn;

        do
        {
            vkn = FakerHelper.Vkn();
        }
        while (!kullanilanVkn.Add(vkn));

        return vkn;
    }
}
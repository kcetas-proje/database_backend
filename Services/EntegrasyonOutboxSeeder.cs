using Bogus;
using KcetasAboneApi.Models;
using Microsoft.EntityFrameworkCore;

namespace KcetasAboneApi.Services;

public class EntegrasyonOutboxSeeder
{
    private readonly AppDbContext _context;

    public EntegrasyonOutboxSeeder(AppDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync(int adet = 500)
    {
        if (await _context.EntegrasyonOutboxes.AnyAsync())
        {
            Console.WriteLine("EntegrasyonOutbox tablosunda kayıt bulunduğu için yeni kayıt üretilmedi.");
            return;
        }

        var faturalar = await _context.Faturas.ToListAsync();

        if (!faturalar.Any())
        {
            Console.WriteLine("Fatura bulunamadığı için EntegrasyonOutbox oluşturulamadı.");
            return;
        }

        var faker = new Faker<EntegrasyonOutbox>("tr")
            .RuleFor(x => x.FaturaId, f => f.PickRandom(faturalar).FaturaId)

            .RuleFor(x => x.HedefSistem, f =>
            {
                var rnd = f.Random.Int(1, 100);

                if (rnd <= 45)
                    return HedefSistem.ERP;
                if (rnd <= 70)
                    return HedefSistem.GIB_EFATURA;
                if (rnd <= 85)
                    return HedefSistem.GIB_EARSIV;
                if (rnd <= 95)
                    return HedefSistem.CRM_NOTIFICATION;

                return HedefSistem.FIELD_PRINT;
            })

            .RuleFor(x => x.IdempotencyKey, f => Guid.NewGuid().ToString())
            .RuleFor(x => x.CorrelationId, f => Guid.NewGuid().ToString())
            .RuleFor(x => x.Payload, (f, x) =>
                $$"""
                {
                    "FaturaId": {{x.FaturaId}},
                    "OlusturmaTarihi": "{{DateTime.UtcNow:O}}"
                }
                """)

            .RuleFor(x => x.CreatedAt, f => f.Date.Past(1))

            .RuleFor(x => x.Durum, f =>
            {
                var rnd = f.Random.Int(1, 100);

                if (rnd <= 70)
                    return OutboxDurumu.GONDERILDI;

                if (rnd <= 85)
                    return OutboxDurumu.BEKLIYOR;

                if (rnd <= 95)
                    return OutboxDurumu.HATA;

                return OutboxDurumu.MANUEL_MUDAHALE;
            });

        var liste = faker.Generate(adet);

        foreach (var item in liste)
        {
            switch (item.Durum)
            {
                case OutboxDurumu.GONDERILDI:
                    item.RetryCount = 0;
                    item.GonderimZamani = item.CreatedAt.AddMinutes(Random.Shared.Next(1, 30));
                    item.SonDenemeTarihi = item.GonderimZamani;
                    item.HataKodu = null;
                    item.HataMesaji = null;
                    break;

                case OutboxDurumu.BEKLIYOR:
                    item.RetryCount = 0;
                    item.GonderimZamani = null;
                    item.SonDenemeTarihi = null;
                    item.HataKodu = null;
                    item.HataMesaji = null;
                    break;

                case OutboxDurumu.HATA:
                    item.RetryCount = Random.Shared.Next(1, 6);
                    item.GonderimZamani = null;
                    item.SonDenemeTarihi = item.CreatedAt.AddMinutes(Random.Shared.Next(5, 120));
                    item.HataKodu = "HTTP500";
                    item.HataMesaji = "Hedef sisteme gönderim başarısız.";
                    break;

                case OutboxDurumu.MANUEL_MUDAHALE:
                    item.RetryCount = Random.Shared.Next(3, 10);
                    item.GonderimZamani = null;
                    item.SonDenemeTarihi = item.CreatedAt.AddMinutes(Random.Shared.Next(30, 180));
                    item.HataKodu = "MANUAL";
                    item.HataMesaji = "Kayıt manuel inceleme bekliyor.";
                    break;
            }
        }

        try
        {
            await _context.EntegrasyonOutboxes.AddRangeAsync(liste);
            await _context.SaveChangesAsync();

            Console.WriteLine($"{liste.Count} adet EntegrasyonOutbox oluşturuldu.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("HATA:");
            Console.WriteLine(ex.ToString());
        }
    }
}

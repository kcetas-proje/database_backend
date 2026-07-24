using KcetasAboneApi.Models;

namespace KcetasAboneApi.Services;

public class DatabaseSeeder
{
    private readonly AppDbContext _context;

    public DatabaseSeeder(AppDbContext context)
    {
        _context = context;
    }

    public async Task SeedAbonelerAsync(int adet)
    {
        Console.WriteLine("Aboneler oluşturuluyor...");

        await new AboneSeeder(_context).Generate(adet);

        Console.WriteLine("Aboneler oluşturuldu.");
    }

    public async Task SeedTuketimNoktalariAsync(int adet)
    {
        Console.WriteLine("Tüketim noktaları oluşturuluyor...");

        await new TuketimNoktasiSeeder(_context).Generate(adet);

        Console.WriteLine("Tüketim noktaları oluşturuldu.");
    }

    public async Task SeedSayaclarAsync()
    {
        Console.WriteLine("Sayaçlar oluşturuluyor...");

        await new SayacSeeder(_context).Generate();

        Console.WriteLine("Sayaçlar oluşturuldu.");
    }

    public async Task SeedSozlesmelerAsync(int adet)
    {
        Console.WriteLine("Sözleşmeler oluşturuluyor...");

        await new SozlesmeSeeder(_context).Generate(adet);

        Console.WriteLine("Sözleşmeler oluşturuldu.");
    }

    public async Task SeedIsEmirleriAsync(int adet)
    {
        Console.WriteLine("İş emirleri oluşturuluyor...");

        await new IsEmriSeeder(_context).Generate(adet);

        Console.WriteLine("İş emirleri oluşturuldu.");
    }

    public async Task SeedEntegrasyonOutboxAsync(int adet = 500)
    {
        Console.WriteLine("Entegrasyon Outbox kayıtları oluşturuluyor...");

        var entegrasyonOutboxSeeder = new EntegrasyonOutboxSeeder(_context);
        await entegrasyonOutboxSeeder.SeedAsync(adet);

        Console.WriteLine("Entegrasyon Outbox kayıtları oluşturuldu.");
    }
}

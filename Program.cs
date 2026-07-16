using Microsoft.EntityFrameworkCore;
using KcetasAboneApi.Models;
using KcetasAboneApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer; 
using Microsoft.IdentityModel.Tokens; 
using System.Text;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.AddScoped<JwtService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"] ?? ""))
        };
    });
builder.Services.AddAuthorization();


builder.Services.AddHttpClient();
builder.Services.AddHostedService<OutboxWorkerService>(); 
builder.Services.AddHostedService<SayacAvcisiWorkerService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Kcetas Sistem API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
{
    Type = SecuritySchemeType.Http,
    Scheme = "bearer",
    BearerFormat = "JWT",
    Name = "Authorization",
    In = ParameterLocation.Header,
    Description = "JWT Authorization header using the Bearer scheme."
});

c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
{
    [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
});

});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => { policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(); });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    // Veritabanı ile Kod arasındaki uyuşmazlığı çözmek için eksik kolonu ekliyoruz
    try 
    {
        db.Database.ExecuteSqlRaw("ALTER TABLE is_emirleri ADD COLUMN IF NOT EXISTS acma_noktasi text;");
        Console.WriteLine("--> LOKAL ORTAM: 'acma_noktasi' kolonu başarıyla eklendi veya zaten var.");
    }
    catch (Exception ex)
    {
        Console.WriteLine("--> LOKAL ORTAM: Kolon eklenirken bir hata oluştu: " + ex.Message);
    }

    // Sadece geliştirme (local) ortamında ana tablolar boşsa sahte verilerle dolduralım
    if (app.Environment.IsDevelopment() && !db.Abonelers.Any())
    {
        // 1. Aboneleri Üret
        var aboneFaker = new Bogus.Faker<Aboneler>("tr")
            .RuleFor(a => a.AboneNo, f => "ABN-" + f.Random.Number(100000, 999999))
            .RuleFor(a => a.AboneTipi, f => f.PickRandom("BIREYSEL", "KURUMSAL"))
            .RuleFor(a => a.Ad, f => f.Name.FirstName())
            .RuleFor(a => a.Soyad, f => f.Name.LastName())
            .RuleFor(a => a.Tckn, f => f.Random.String2(11, "0123456789"))
            .RuleFor(a => a.Telefon, f => f.Phone.PhoneNumber("5##-###-##-##"))
            .RuleFor(a => a.CreatedAt, DateTime.UtcNow);
        var aboneler = aboneFaker.Generate(10);
        db.Abonelers.AddRange(aboneler);
        db.SaveChanges();
        Console.WriteLine("--> LOKAL ORTAM: 10 Adet Sahte Abone Eklendi!");

        // İl, İlçe ve Tarife Yoksa Varsayılan Ekleyelim
        if (!db.Tarifelers.Any()) { db.Tarifelers.Add(new Tarifeler { TarifeKodu = "T01", TarifeAdi = "Mesken", GunduzBirimFiyat = 1.5m, KdvOrani = 20m, DagitimBedeli = 0.5m, Aktif = true, CreatedAt = DateTime.UtcNow }); db.SaveChanges(); }
        if (!db.Ils.Any()) { db.Ils.Add(new Il { IlAdi = "Kayseri" }); db.SaveChanges(); }
        if (!db.Ilces.Any()) { db.Ilces.Add(new Ilce { IlId = 1, IlceAdi = "Melikgazi" }); db.SaveChanges(); }

        var tarifeId = db.Tarifelers.First().TarifeId;
        var ilceId = db.Ilces.First().IlceId;

        // 2. Tüketim Noktalarını Üret
        var tnFaker = new Bogus.Faker<TuketimNoktasi>("tr")
            .RuleFor(t => t.TekilKod, f => "TN-" + f.Random.Number(100000, 999999))
            .RuleFor(t => t.IlceId, ilceId)
            .RuleFor(t => t.Mahalle, f => f.Address.StreetName() + " Mah.")
            .RuleFor(t => t.AcikAdres, f => f.Address.FullAddress())
            .RuleFor(t => t.BaglantiGucuKw, f => Math.Round(f.Random.Decimal(3, 15), 1))
            .RuleFor(t => t.TuketiciGrubu, "MESKEN")
            .RuleFor(t => t.BaglantiDurumu, "AKTIF")
            .RuleFor(t => t.Status, "AKTIF")
            .RuleFor(t => t.CreatedAt, DateTime.UtcNow);
        var tuketimNoktalari = tnFaker.Generate(10);
        db.TuketimNoktasis.AddRange(tuketimNoktalari);
        db.SaveChanges();
        Console.WriteLine("--> LOKAL ORTAM: 10 Adet Tüketim Noktası Eklendi!");

        // 3. Sözleşmeleri Üret
        var sozlesmeFaker = new Bogus.Faker<Sozlesmeler>("tr")
            .RuleFor(s => s.SozlesmeNo, f => "SOZ-" + f.Random.Number(10000, 99999))
            .RuleFor(s => s.TarifeId, tarifeId)
            .RuleFor(s => s.SozlesmeTipi, "MESKEN")
            .RuleFor(s => s.Durum, "AKTIF")
            .RuleFor(s => s.BaslangicTarihi, DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-6)))
            .RuleFor(s => s.GuvenceBedeli, 450)
            .RuleFor(s => s.CreatedAt, DateTime.UtcNow);
        
        for (int i = 0; i < 10; i++)
        {
            var sozlesme = sozlesmeFaker.Generate();
            sozlesme.AboneId = aboneler[i].AboneId;
            sozlesme.TuketimNoktasiId = tuketimNoktalari[i].TuketimNoktasiId;
            db.Sozlesmelers.Add(sozlesme);
        }
        db.SaveChanges();

        // 4. Takılı Sayaçları Üret
        var sayacFaker = new Bogus.Faker<Sayaclar>("tr")
            .RuleFor(s => s.SeriNo, f => "SNC-" + f.Random.Number(1000000, 9999999).ToString())
            .RuleFor(s => s.Marka, f => f.PickRandom("Makel", "Luna", "Viko", "Köhler"))
            .RuleFor(s => s.UretimYili, f => f.Random.Number(2015, 2024))
            .RuleFor(s => s.Durum, "TAKILI")
            .RuleFor(s => s.CreatedAt, DateTime.UtcNow);

        for (int i = 0; i < 10; i++)
        {
            var sayac = sayacFaker.Generate();
            sayac.TuketimNoktasiId = tuketimNoktalari[i].TuketimNoktasiId;
            db.Sayaclars.Add(sayac);
        }
        db.SaveChanges();
        Console.WriteLine("--> LOKAL ORTAM: 10 Adet TAKILI Sayaç ve Sözleşme Eklendi!");
    }

    // Sadece geliştirme (local) ortamında ve eğer fatura tablosu boşsa sahte veri üretelim
    if (app.Environment.IsDevelopment() && !db.Faturas.Any())
    {
        var mevcutSozlesmeler = db.Sozlesmelers.Select(s => s.SozlesmeId).ToList();
        
        // Fatura kesebilmemiz için sistemde en az 1 sözleşme olması lazım
        if (mevcutSozlesmeler.Any())
        {
            var faker = new Bogus.Faker<Fatura>("tr")
                .RuleFor(f => f.FaturaNo, f => "FAT-" + f.Random.Number(100000, 999999))
                .RuleFor(f => f.SozlesmeId, f => f.PickRandom(mevcutSozlesmeler))
                .RuleFor(f => f.TekilKod, f => f.Random.String2(10, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"))
                .RuleFor(f => f.FaturaTipi, f => "DONEM")
                .RuleFor(f => f.Donem, f => DateTime.UtcNow.ToString("yyyyMM"))
                .RuleFor(f => f.FaturaTarihi, f => DateOnly.FromDateTime(f.Date.Recent(30)))
                .RuleFor(f => f.SonOdemeTarihi, (f, u) => u.FaturaTarihi.AddDays(10))
                .RuleFor(f => f.TuketimKwh, f => Math.Round(f.Random.Decimal(50, 500), 2))
                .RuleFor(f => f.ToplamTutar, f => Math.Round(f.Random.Decimal(100, 1500), 2))
                .RuleFor(f => f.EnerjiBedeli, (f, u) => Math.Round(u.ToplamTutar * 0.50m, 2))
                .RuleFor(f => f.DagitimBedeli, (f, u) => Math.Round(u.ToplamTutar * 0.30m, 2))
                .RuleFor(f => f.VergiFonToplam, (f, u) => Math.Round(u.ToplamTutar * 0.20m, 2))
                .RuleFor(f => f.Durum, f => f.PickRandom("HESAPLANDI", "ONAYLANDI", "ODENDI"))
                .RuleFor(f => f.Status, f => "AKTIF")
                .RuleFor(f => f.CreatedAt, f => DateTime.UtcNow);

            var sahteFaturalar = faker.Generate(25); // 25 adet sahte fatura
            db.Faturas.AddRange(sahteFaturalar);
            db.SaveChanges();
            
            Console.WriteLine("--> LOKAL ORTAM: 25 Adet Sahte Fatura (Bogus) Başarıyla Eklendi!");
        }
        else 
        {
            Console.WriteLine("--> LOKAL ORTAM: Fatura üretilemedi çünkü sistemde hiç 'Sözleşme' yok.");
        }
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");
app.UseHttpsRedirection();

// Global Exception Handler Middleware'i sisteme dahil ediyoruz
app.UseMiddleware<KcetasAboneApi.Middlewares.ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization(); 

app.MapControllers(); 

// Hata yönetimini test edebilmeniz için geçici bir test endpointi
app.MapGet("/api/test-error", () =>
{
    throw new Exception("Bu bilerek fırlatılan bir test hatasıdır!");
});

app.Run();
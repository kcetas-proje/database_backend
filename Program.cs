using FluentValidation;
using FluentValidation.AspNetCore;
using KcetasAboneApi.Hubs;
using KcetasAboneApi.Models;
using KcetasAboneApi.Services;
using KcetasSeeder.Seeders;
using Microsoft.AspNetCore.Authentication.JwtBearer; 
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens; 
using Microsoft.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});
builder.Services.AddScoped<IAboneService, AboneService>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddSignalR();
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

// Veritabanı yoksa oluştur
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// 💥 GIGACHAD SEEDER KONTROLÜ (Kurşun Geçirmez Versiyon)
var komutlar = Environment.GetCommandLineArgs();
if (komutlar.Any(k => k.ToLower().Contains("seed")))
{
    await RunSeederMenu(app.Services);
    return; 
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");
app.UseHttpsRedirection();

// Global Exception Handler Middleware
app.UseMiddleware<KcetasAboneApi.Middlewares.ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseMiddleware<KcetasAboneApi.Middlewares.UserContextMiddleware>();
app.UseAuthorization();

app.MapHub<BildirimHub>("/bildirimHub");

app.MapControllers(); 

// Hata yönetimini test endpointi
app.MapGet("/api/test-error", () =>
{
    throw new Exception("Bu bilerek fırlatılan bir test hatasıdır!");
});

// 🔥 VERİTABANINDAKİ GEÇERSİZ SAYAÇ DURUMLARINI DÜZELT
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.ExecuteSqlRaw("UPDATE sayaclar SET durum = 'TAKILI' WHERE durum = 'AKTIF';");
}

// 🔥 WEB API BURADA ATEŞLENİYOR
app.Run();

// ========================================================================
// 🏗️ ŞANTİYE TOHUMLAMA (SEEDER) METODU 
// ========================================================================
static async Task RunSeederMenu(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    while (true)
    {
        Console.Clear();
        Console.WriteLine("==========================================");
        Console.WriteLine("          KCETAŞ RANDOM DATA SEEDER");
        Console.WriteLine("==========================================");
        Console.WriteLine();
        Console.WriteLine("1 - Aboneler");
        Console.WriteLine("2 - Tüketim Noktaları");
        Console.WriteLine("3 - Sayaçlar");
        Console.WriteLine("4 - Sözleşmeler");
        Console.WriteLine("5 - Endeks Okuma");
        Console.WriteLine("6 - Faturalar");
        Console.WriteLine("7 - İş Emirleri");
        Console.WriteLine("8 - Entegrasyon Outbox");
        Console.WriteLine("0 - Çıkış");
        Console.WriteLine();
        Console.Write("Seçiminiz : ");
        
        string? secim = Console.ReadLine();

        if (secim == "0")
        {
            Console.WriteLine("Program sonlandırıldı.");
            break;
        }

        int adet = 0;

        switch (secim)
        {
            case "1":
                Console.Write("Kaç adet abone üretilecek : ");
                if (int.TryParse(Console.ReadLine(), out adet) && adet > 0)
                {
                    Console.WriteLine("\nAboneler oluşturuluyor...\n");
                    await new AboneSeeder(context).Generate(adet);
                }
                else Console.WriteLine("Geçerli bir sayı giriniz.");
                break;
            case "2":
                Console.Write("Kaç adet tüketim noktası üretilecek : ");
                if (int.TryParse(Console.ReadLine(), out adet) && adet > 0)
                {
                    Console.WriteLine("\nTüketim noktaları oluşturuluyor...\n");
                    await new TuketimNoktasiSeeder(context).Generate(adet);
                }
                else Console.WriteLine("Geçerli bir sayı giriniz.");
                break;
            case "3":
                Console.WriteLine("\nSayaçlar oluşturuluyor...\n");
                await new SayacSeeder(context).Generate();
                break;
            case "4":
                Console.Write("Kaç adet sözleşme üretilecek : ");
                if (int.TryParse(Console.ReadLine(), out adet) && adet > 0)
                {
                    Console.WriteLine("\nSözleşmeler oluşturuluyor...\n");
                    await new SozlesmeSeeder(context).Generate(adet);
                }
                else Console.WriteLine("Geçerli bir sayı giriniz.");
                break;
            case "5":
                Console.Write("Kaç aylık endeks oluşturulsun : ");
                if (int.TryParse(Console.ReadLine(), out adet) && adet > 0)
                {
                    Console.WriteLine("\nEndeks okumaları oluşturuluyor...\n");
                    await new EndeksOkumaSeeder(context).Generate(adet);
                }
                else Console.WriteLine("Geçerli bir sayı giriniz.");
                break;
            case "6":
                Console.WriteLine("\nFaturalar oluşturuluyor...\n");
                await new FaturaSeeder(context).Generate();
                break;
            case "7":
            Console.Write("Kaç adet iş emri üretilecek : ");

            if (!int.TryParse(Console.ReadLine(), out adet) || adet <= 0)
            {
                Console.WriteLine("Geçerli bir sayı giriniz.");
                Console.ReadKey();
                continue;
            }

            Console.WriteLine("\nİş emirleri oluşturuluyor...\n");
            await new IsEmriSeeder(context).Generate(adet);

            break;
            case "8":
                Console.Write("Kaç adet Entegrasyon Outbox üretilecek : ");

                if (int.TryParse(Console.ReadLine(), out adet) && adet > 0)
                {
                    Console.WriteLine("\nEntegrasyon Outbox kayıtları oluşturuluyor...\n");
                    await new EntegrasyonOutboxSeeder(context).SeedAsync(adet);
                }
                else
                {
                    Console.WriteLine("Geçerli bir sayı giriniz.");
                }

                break;

            default: 
            Console.WriteLine("Geçersiz seçim yaptınız."); 
            Console.ReadKey(); 
            continue; 
    }
        }

        Console.WriteLine("\n==========================================");
        Console.WriteLine("İşlem tamamlandı veya hata alındı.");
        Console.WriteLine("==========================================");
        Console.WriteLine("Devam etmek için bir tuşa basınız...");
        Console.ReadKey();
    }

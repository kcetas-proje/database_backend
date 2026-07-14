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
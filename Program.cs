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

// --- JWT Yetkilendirme Ayarları ---
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

// Her başlatmada tabloları modellerle senkronize et
// (pull sonrası schema değişikliklerini otomatik uygular)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    // Schema değişikliği kontrolü için modelin hash'ini hesapla
    var script = db.Database.GenerateCreateScript();
    var hashBytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(script));
    var currentHash = Convert.ToBase64String(hashBytes);

    bool schemaChanged = true;
    try 
    {
        using var command = db.Database.GetDbConnection().CreateCommand();
        command.CommandText = "SELECT \"Hash\" FROM \"_SchemaHash\" WHERE \"Id\" = 1";
        db.Database.OpenConnection();
        var result = command.ExecuteScalar()?.ToString();
        if (result == currentHash)
        {
            schemaChanged = false; // Schema aynı, verileri silmeye gerek yok
        }
    }
    catch 
    {
        // Tablo muhtemelen henüz yok veya hata alındı, schemaChanged = true kalır
    }

    if (schemaChanged)
    {
        // Schema değişmiş (veya ilk defa çalışıyor), mevcut tabloları sil ve yeniden oluştur
        db.Database.ExecuteSqlRaw(@"
            DO $$ DECLARE
                r RECORD;
            BEGIN
                FOR r IN (SELECT tablename FROM pg_tables WHERE schemaname = 'public') LOOP
                    EXECUTE 'DROP TABLE IF EXISTS public.' || quote_ident(r.tablename) || ' CASCADE';
                END LOOP;
            END $$;
        ");
        
        db.Database.EnsureCreated();

        // Yeni hash'i kaydet
        db.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS ""_SchemaHash"" (""Id"" INT PRIMARY KEY, ""Hash"" TEXT);
            INSERT INTO ""_SchemaHash"" (""Id"", ""Hash"") VALUES (1, {0})
            ON CONFLICT (""Id"") DO UPDATE SET ""Hash"" = {0};
        ", currentHash);
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization(); 

app.MapControllers(); 

app.Run();
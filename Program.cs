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


app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization(); 

app.MapControllers(); 

app.Run();
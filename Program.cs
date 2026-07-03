using Microsoft.EntityFrameworkCore;
using KcetasAboneApi.Models;
using Microsoft.OpenApi;
using KcetasAboneApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddHttpClient();
builder.Services.AddHostedService<OutboxWorkerService>(); 

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHostedService<OutboxWorkerService>();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Kcetas Sistem API", 
        Version = "v1",
        Description = "KCETAŞ Operasyonel API Yönetim Paneli"
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())


    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Kcetas Sistem API v1");
    });


app.UseHttpsRedirection();
app.MapControllers(); 

app.Run();
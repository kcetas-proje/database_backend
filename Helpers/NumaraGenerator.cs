using KcetasAboneApi.Models;
using Microsoft.EntityFrameworkCore;

namespace KcetasSeeder.Helpers;

public static class NumaraGenerator
{
    // Tüketim Noktası
    public static async Task<int> GetLastNumber(AppDbContext context)
    {
        var sonKod = await context.TuketimNoktasis
            .OrderByDescending(x => x.TuketimNoktasiId)
            .Select(x => x.TekilKod)
            .FirstOrDefaultAsync();

        if (string.IsNullOrWhiteSpace(sonKod))
            return 0;

        var parcalar = sonKod.Split('-');

        if (parcalar.Length != 3)
            return 0;

        return int.Parse(parcalar[2]);
    }

    // Sözleşme
    public static async Task<int> GetLastSozlesmeNumber(AppDbContext context)
    {
        var sonId = await context.Sozlesmelers
            .MaxAsync(x => (long?)x.SozlesmeId);

        return (int)(sonId ?? 0);
    }

    // Fatura
    public static async Task<int> GetLastFaturaNumber(AppDbContext context)
    {
        var sonId = await context.Faturas
            .MaxAsync(x => (long?)x.FaturaId);

        return (int)(sonId ?? 0);
    }
    // Abone
    public static async Task<int> GetLastAboneNumber(AppDbContext context)
    {
        var sonId = await context.Abonelers
            .MaxAsync(x => (long?)x.AboneId);

        return (int)(sonId ?? 0);
    }
    // Sayaç
    public static async Task<int> GetLastSayacNumber(AppDbContext context)
    {
        var sonId = await context.Sayaclars
            .MaxAsync(x => (long?)x.SayacId);

        return (int)(sonId ?? 0);

    }
    // KcetasSeeder/Helpers/NumaraGenerator.cs
    public static async Task<int> GetLastIsEmriNumber(AppDbContext context)
    {
        var sonId = await context.IsEmirleris
            .MaxAsync(x => (long?)x.IsEmriId);

        return (int)(sonId ?? 0);
    }

}

using KcetasAboneApi.Models;
using KcetasAboneApi.Models.Dtos;
using KcetasAboneApi.Services;
using Microsoft.EntityFrameworkCore;

public class AboneService : IAboneService
{
    private readonly AppDbContext _context;

    public AboneService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AboneFaturaResponseDto> GetFaturalar(
        string? ad,
        int page,
        int pageSize)
    {
        var query = _context.Faturas
            .Include(f => f.Sozlesme)
                .ThenInclude(s => s.Abone)
            .AsQueryable();

        // İsim ile filtreleme
        if (!string.IsNullOrWhiteSpace(ad))
        {
            ad = ad.Trim().ToLower();

            query = query.Where(f =>
                (
                    ((f.Sozlesme.Abone!.Ad ?? "") + " " + (f.Sozlesme.Abone.Soyad ?? ""))
                        .ToLower()
                        .Contains(ad)
                )
                ||
                (
                    (f.Sozlesme.Abone.Unvan ?? "")
                        .ToLower()
                        .Contains(ad)
                ));
        }

        // Toplam kayıt sayısı
        var totalCount = await query.CountAsync();

        // İstenen sayfadaki kayıtlar
        var liste = await query
            .OrderByDescending(f => f.FaturaTarihi)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new AboneFaturaDto
            {
                AboneNo = f.Sozlesme.Abone!.AboneNo,

                AdSoyad = f.Sozlesme.Abone.AboneTipi == "BIREYSEL"
                    ? $"{f.Sozlesme.Abone.Ad} {f.Sozlesme.Abone.Soyad}"
                    : f.Sozlesme.Abone.Unvan,

                FaturaNo = f.FaturaNo,

                Donem = f.Donem,

                FaturaTarihi = f.FaturaTarihi,

                ToplamTutar = f.ToplamTutar,

                Durum = f.Durum.ToString()
            })
            .ToListAsync();

        // Response
        return new AboneFaturaResponseDto
        {
            TotalCount = totalCount,
            CurrentPage = page,
            PageSize = pageSize,
            HasNextPage = page * pageSize < totalCount,
            Data = liste
        };
    }
}

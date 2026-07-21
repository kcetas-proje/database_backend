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
        string? isim,
        int page,
        int pageSize)
    {
        if (page < 1)
            page = 1;

        if (pageSize < 1)
            pageSize = 10;

        var query = _context.Faturas
            .Include(f => f.Sozlesme)
                .ThenInclude(s => s.Abone)
            .Where(f => f.Sozlesme.Abone != null)
            .AsQueryable();

        // İsim ile filtreleme
        if (!string.IsNullOrWhiteSpace(isim))
        {
            isim = isim.Trim();

            query = query.Where(f =>

                EF.Functions.ILike(f.Sozlesme.Abone!.Ad ?? "", $"%{isim}%")

                ||

                EF.Functions.ILike(f.Sozlesme.Abone.Soyad ?? "", $"%{isim}%")

                ||

                EF.Functions.ILike(
                    (f.Sozlesme.Abone.Ad ?? "") + " " + (f.Sozlesme.Abone.Soyad ?? ""),
                    $"%{isim}%")

                ||

                EF.Functions.ILike(f.Sozlesme.Abone.Unvan ?? "", $"%{isim}%")
            );
        }

        var totalCount = await query.CountAsync();

        var liste = await query
            .OrderByDescending(f => f.FaturaTarihi)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new AboneFaturaDto
            {
                AboneNo = f.Sozlesme.Abone!.AboneNo,

                AdSoyad = !string.IsNullOrWhiteSpace(f.Sozlesme.Abone.Ad)
                    ? $"{f.Sozlesme.Abone.Ad} {f.Sozlesme.Abone.Soyad}"
                    : f.Sozlesme.Abone.Unvan ?? "",

                FaturaNo = f.FaturaNo,

                Donem = f.Donem,

                FaturaTarihi = f.FaturaTarihi,

                ToplamTutar = f.ToplamTutar,

                Durum = f.Durum.ToString()
            })
            .ToListAsync();

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

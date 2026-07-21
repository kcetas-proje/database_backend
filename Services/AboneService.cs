using KcetasAboneApi.Models;
using KcetasAboneApi.Services;
using Microsoft.EntityFrameworkCore;

public class AboneService : IAboneService
{
    private readonly AppDbContext _context;

    public AboneService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AboneListResponseDto> GetAboneler(
        string? isim,
        int page,
        int pageSize)
    {
        var query = _context.Abonelers.AsQueryable();

        // İsim ile filtreleme
        if (!string.IsNullOrWhiteSpace(isim))
        {
            isim = isim.Trim().ToLower();

            query = query.Where(a =>
                (
                    ((a.Ad ?? "") + " " + (a.Soyad ?? ""))
                        .ToLower()
                        .Contains(isim)
                )
                ||
                (
                    (a.Unvan ?? "")
                        .ToLower()
                        .Contains(isim)
                ));
        }

        var totalCount = await query.CountAsync();

        var liste = await query
            .OrderBy(a => a.AboneId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AboneResponseDto
            {
                AboneId = a.AboneId,
                AboneNo = a.AboneNo,
                AboneTipi = a.AboneTipi,
                Ad = a.Ad ?? "",
                Soyad = a.Soyad ?? "",
                Unvan = a.Unvan ?? "",
                Telefon = a.Telefon ?? "",
                EPosta = a.EPosta ?? ""
            })
            .ToListAsync();

        return new AboneListResponseDto
        {
            TotalCount = totalCount,
            CurrentPage = page,
            PageSize = pageSize,
            HasNextPage = page * pageSize < totalCount,
            Data = liste
        };
    }
}

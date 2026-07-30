using KcetasAboneApi.Models;
using KcetasAboneApi.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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

        var query = _context.Abonelers.AsNoTracking().AsQueryable();

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
                EPosta = a.EPosta ?? "",
                Status = a.Status
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

    public async Task<AboneFaturaResponseDto> GetAboneFaturalari(
    long aboneId,
    int page,
    int pageSize)
    {

        var query = _context.Faturas
            .AsNoTracking() 
            .Include(f => f.Sozlesme)
            .Where(f => f.Sozlesme.AboneId == aboneId);

        var totalCount = await query.CountAsync();

        var liste = await query
            .OrderByDescending(f => f.FaturaTarihi)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new AboneFaturaDto
            {
                FaturaId = f.FaturaId,
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
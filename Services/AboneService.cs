using Microsoft.EntityFrameworkCore;
using KcetasAboneApi.Models;

public class AboneService : IAboneService
{
    private readonly AppDbContext _context;

    public AboneService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<AboneFaturaDto>> GetSon10Fatura(string ilkUcHane)
    {
        return await _context.Faturas

            .Include(x => x.Sozlesme)
            .ThenInclude(x => x.Abone)

            .Where(x =>
                x.Sozlesme.Abone.AboneNo.StartsWith(ilkUcHane))

            .OrderByDescending(x => x.FaturaTarihi)

            .Take(10)

            .Select(x => new AboneFaturaDto
            {
                AboneNo = x.Sozlesme.Abone.AboneNo,

                AdSoyad =
                    x.Sozlesme.Abone.AboneTipi == "GERCEK"
                    ? x.Sozlesme.Abone.Ad + " " + x.Sozlesme.Abone.Soyad
                    : x.Sozlesme.Abone.Unvan,

                FaturaNo = x.FaturaNo,

                Donem = x.Donem,

                FaturaTarihi = x.FaturaTarihi,

                ToplamTutar = x.ToplamTutar,

                Durum = x.Durum.ToString()
            })

            .ToListAsync();
    }
}

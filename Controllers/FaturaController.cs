using KcetasAboneApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KcetasAboneApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FaturaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FaturaController(AppDbContext context)
        {
            _context = context;
        }

      
        [HttpGet]
        public async Task<IActionResult> GetFaturalar()
        {
            var faturalar = await _context.Faturas
                .Where(f => f.Status == "AKTIF")
                .ToListAsync();

            if (!faturalar.Any())
            {
                return NotFound("Sistemde aktif fatura bulunamadı.");
            }

            return Ok(faturalar);
        }

        [HttpPost]
        public async Task<IActionResult> YeniFaturaEkle([FromBody] Fatura yeniFatura)
        {
            yeniFatura.Status = "AKTIF";
            yeniFatura.CreatedAt = DateTime.UtcNow;

            _context.Faturas.Add(yeniFatura);
            await _context.SaveChangesAsync();

            return Ok(yeniFatura);
        }

        
        [HttpPut("{id}")]
        public async Task<IActionResult> FaturaGuncelle(long id, [FromBody] Fatura guncelFatura)
        {
            var dbFatura = await _context.Faturas.FindAsync(id);

            if (dbFatura == null)
            {
                return NotFound("Böyle bir fatura bulunamadı.");
            }

            dbFatura.FaturaNo = guncelFatura.FaturaNo;
            dbFatura.SozlesmeId = guncelFatura.SozlesmeId;
            dbFatura.TekilKod = guncelFatura.TekilKod;
            dbFatura.FaturaTipi = guncelFatura.FaturaTipi;
            dbFatura.Donem = guncelFatura.Donem;
            dbFatura.FaturaTarihi = guncelFatura.FaturaTarihi;
            dbFatura.SonOdemeTarihi = guncelFatura.SonOdemeTarihi;
            dbFatura.OkumaId = guncelFatura.OkumaId;
            dbFatura.IlkEndeks = guncelFatura.IlkEndeks;
            dbFatura.SonEndeks = guncelFatura.SonEndeks;
            dbFatura.TuketimKwh = guncelFatura.TuketimKwh;
            dbFatura.ReaktifEnduktif = guncelFatura.ReaktifEnduktif;
            dbFatura.ReaktifKapasitif = guncelFatura.ReaktifKapasitif;
            dbFatura.Carpan = guncelFatura.Carpan;
            dbFatura.EnerjiBedeli = guncelFatura.EnerjiBedeli;
            dbFatura.DagitimBedeli = guncelFatura.DagitimBedeli;
            dbFatura.HizmetBedeli = guncelFatura.HizmetBedeli;
            dbFatura.KesmeBaglamaBedeli = guncelFatura.KesmeBaglamaBedeli;
            dbFatura.VergiFonToplam = guncelFatura.VergiFonToplam;
            dbFatura.ToplamTutar = guncelFatura.ToplamTutar;
            dbFatura.Durum = guncelFatura.Durum;
            dbFatura.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(dbFatura);
        }

        
        [HttpDelete("{id}")]
        public async Task<IActionResult> FaturaSil(long id)
        {
            var dbFatura = await _context.Faturas.FindAsync(id);

            if (dbFatura == null)
            {
                return NotFound("Böyle bir fatura bulunamadı.");
            }

            dbFatura.Status = "PASIF";
            dbFatura.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mesaj = $"{dbFatura.FaturaNo} numaralı fatura başarıyla pasif duruma alındı."
            });
        }
    }
}

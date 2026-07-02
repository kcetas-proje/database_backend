using KcetasAboneApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

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
        public async Task<IActionResult> YeniFaturaEkle([FromBody] FaturaCreateDto dto)
        {
            string rasgeleFaturaNo = "FAT" + DateTime.Now.ToString("yyyyMMddHHmmss");
            string rasgeleTekilKod = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();

            var yeniFatura = new Fatura
            {
                SozlesmeId = dto.SozlesmeId,
                OkumaId = dto.OkumaId,
                FaturaNo = rasgeleFaturaNo,
                TekilKod = rasgeleTekilKod,
                FaturaTipi = "DONEM",
                Donem = DateTime.Now.ToString("yyyy-MM"),

                FaturaTarihi = DateOnly.FromDateTime(DateTime.UtcNow),
                SonOdemeTarihi = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                
                TuketimKwh = dto.TuketimKwh,
                ToplamTutar = dto.ToplamTutar,

                EnerjiBedeli = dto.ToplamTutar * 0.50m,
                DagitimBedeli = dto.ToplamTutar * 0.30m,
                VergiFonToplam = dto.ToplamTutar * 0.20m,
                HizmetBedeli = 0m,
                KesmeBaglamaBedeli = 0m,
                Carpan = 1m,
                
                Durum = "HESAPLANDI",
                Status = "AKTIF",
                CreatedAt = DateTime.UtcNow
            };

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
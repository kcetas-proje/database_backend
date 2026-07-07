using KcetasAboneApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace KcetasAboneApi.Controllers
{   
    //[Authorize(Roles = "1, 6")]
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

                Donem = string.IsNullOrEmpty(dto.Donem) ? DateTime.Now.ToString("yyyy-MM") : dto.Donem,
                
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

            // Outbox (Kargo) Payload hazırlığı
            var payloadData = new 
            {
                FaturaNo = yeniFatura.FaturaNo,
                Tarih = yeniFatura.FaturaTarihi,
                Tutar = yeniFatura.ToplamTutar,
                AboneSözlesme = yeniFatura.SozlesmeId
            };

            var outboxKaydi = new EntegrasyonOutbox
            {
                Fatura = yeniFatura, 
                HedefSistem = "GIB_EFATURA",
                IdempotencyKey = Guid.NewGuid().ToString(), 
                Payload = JsonSerializer.Serialize(payloadData),
                Durum = "BEKLIYOR",
                RetryCount = 0,
                CreatedAt = DateTime.UtcNow
            };

            _context.EntegrasyonOutboxes.Add(outboxKaydi); 

            // Transaction: İkisi aynı anda veritabanına basılır
            await _context.SaveChangesAsync();

            return Ok(yeniFatura);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] FaturaUpdateDto dto)
        {
            if (id != dto.FaturaId) return BadRequest("ID uyuşmazlığı!");

            var fatura = await _context.Faturas.FindAsync(id);
            if (fatura == null) return NotFound();

            fatura.FaturaNo = dto.FaturaNo;
            fatura.ToplamTutar = dto.ToplamTutar;
            fatura.Durum = dto.Durum;
            fatura.SonOdemeTarihi = dto.SonOdemeTarihi;
            fatura.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
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
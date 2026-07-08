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

        [HttpPost("hesapla/{endeksOkumaId}")]
        public async Task<ActionResult> FaturaKes(long endeksOkumaId)
        {
            var okuma = await _context.EndeksOkumas.FindAsync(endeksOkumaId);
            if (okuma == null) 
                return NotFound(new { message = "Okuma kaydı bulunamadı." });

            var sozlesme = await _context.Sozlesmelers
                .Include(s => s.Tarife) 
                .FirstOrDefaultAsync(s => s.SozlesmeId == okuma.SozlesmeId);

            if (sozlesme == null || sozlesme.Tarife == null)
                return BadRequest(new { message = "Sözleşme veya Tarife bulunamadı." });

            decimal tuketimMiktari = Convert.ToDecimal(okuma.YeniEndeks - (okuma.OncekiEndeks ?? 0));
            
            if (tuketimMiktari < 0) 
                return BadRequest(new { message = "Tüketim miktarı negatif olamaz." });

            decimal aktifEnerjiTutari = tuketimMiktari * Convert.ToDecimal(sozlesme.Tarife.GunduzBirimFiyat); 
            decimal dagitimTutari = tuketimMiktari * Convert.ToDecimal(sozlesme.Tarife.DagitimBedeli);
            decimal vergisizToplam = aktifEnerjiTutari + dagitimTutari;

            decimal kdvCarpani = Convert.ToDecimal(sozlesme.Tarife.KdvOrani >= 1 ? (sozlesme.Tarife.KdvOrani / 100m) : sozlesme.Tarife.KdvOrani);
            decimal kdvTutari = vergisizToplam * kdvCarpani;
            decimal genelToplam = Math.Round(vergisizToplam + kdvTutari, 2);

            // 3. FATURA NUMARASI VE DÖNEM ÜRETİMİ
            var buAykiSayi = await _context.Faturas.CountAsync(f => f.FaturaTarihi.Year == DateTime.UtcNow.Year && f.FaturaTarihi.Month == DateTime.UtcNow.Month);
            string faturaNumarasi = $"FAT-{DateTime.UtcNow:yyyyMM}-{(buAykiSayi + 1).ToString("D5")}"; 
            string donemBilgisi = DateTime.UtcNow.ToString("yyyyMM"); 
            string benzersizTekilKod = Guid.NewGuid().ToString("N")[..10].ToUpper(); 

            // 4. FATURAYI VERİTABANINA BASMA
            var yeniFatura = new Fatura
            {
                FaturaNo = faturaNumarasi,
                SozlesmeId = sozlesme.SozlesmeId,
                TekilKod = benzersizTekilKod,
                FaturaTipi = "DONEM",
                Donem = donemBilgisi,
                
                FaturaTarihi = DateOnly.FromDateTime(DateTime.UtcNow),
                SonOdemeTarihi = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                
                OkumaId = okuma.OkumaId,
                IlkEndeks = okuma.OncekiEndeks ?? 0,
                SonEndeks = okuma.YeniEndeks,
                TuketimKwh = tuketimMiktari,
                Carpan = 1.0m, 
                ReaktifEnduktif = 0, 
                ReaktifKapasitif = 0,

                EnerjiBedeli = aktifEnerjiTutari,
                DagitimBedeli = dagitimTutari,
                HizmetBedeli = 0, 
                KesmeBaglamaBedeli = 0,
                VergiFonToplam = kdvTutari, 
                ToplamTutar = genelToplam,
                
                Durum = "HESAPLANDI",
                Status = "AKTIF",
                CreatedAt = DateTime.UtcNow
            };

            _context.Faturas.Add(yeniFatura);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Fatura oluşturuldu!", fatura = yeniFatura });
        }
    }
}
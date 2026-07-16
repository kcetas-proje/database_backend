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

        [HttpGet("All")]
        public async Task<ActionResult<IEnumerable<Fatura>>> GetAllFaturalar()
        {
            return await _context.Faturas
                .Where(f => f.Status == "AKTIF")
                .OrderBy(f => f.FaturaId)
                .ToListAsync();
        }

        [HttpGet]
        public async Task<IActionResult> GetFaturalar([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var query = _context.Faturas.Where(f => f.Status == "AKTIF");

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var faturalar = await query
                .OrderBy(f => f.FaturaId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (totalCount == 0)
            {
                return NotFound("Sistemde aktif fatura bulunamadı.");
            }

            var response = new
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize,
                Data = faturalar
            };

            return Ok(response);
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

    [HttpPost("{faturaId}/onayla")]
    public async Task<IActionResult> FaturaOnayla(long faturaId)
    {
        var fatura = await _context.Faturas.FindAsync(faturaId);
        
        if (fatura == null) 
            return NotFound(new { message = "Fatura bulunamadı!" });

        if (fatura.Durum != "HESAPLANDI")
            return BadRequest(new { message = $"Fatura şu an '{fatura.Durum}' durumunda. Sadece HESAPLANDI olanlar onaylanabilir." });

        // 1. Statüyü güncelliyoruz
        fatura.Durum = "ONAYLANDI";
        fatura.UpdatedAt = DateTime.UtcNow;

        var outboxKargosu = new EntegrasyonOutbox
        {
            FaturaId = fatura.FaturaId,
            HedefSistem = "GIB_EFATURA",
            Durum = "BEKLIYOR",
            IdempotencyKey = Guid.NewGuid().ToString(),
            Payload = System.Text.Json.JsonSerializer.Serialize(new { faturaNo = fatura.FaturaNo, tutar = fatura.ToplamTutar }),
            CreatedAt = DateTime.UtcNow
        };
        
        _context.EntegrasyonOutboxes.Add(outboxKargosu);

        await _context.SaveChangesAsync();

        return Ok(new { message = "Fatura onaylandı ve GİB'e gönderildi!", faturaNo = fatura.FaturaNo });
    }

    [HttpPost("{faturaId}/iptal")]
    public async Task<IActionResult> FaturaIptal(long faturaId)
    {
        var fatura = await _context.Faturas.FindAsync(faturaId);
        
        if (fatura == null) 
            return NotFound(new { message = "Fatura bulunamadı!" });

        if (fatura.Durum == "GONDERILDI" || fatura.Durum == "IPTAL")
            return BadRequest(new { message = "Fatura zaten gönderilmiş veya iptal edilmiş!" });

        fatura.Durum = "IPTAL";
        fatura.Status = "PASIF"; 
        fatura.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Fatura iptal edildi!", faturaNo = fatura.FaturaNo });
    }

    [HttpPost("generate-faturalar")]
    public async Task<IActionResult> GenerateFaturalar()
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var okunmusEndeksler = await _context.EndeksOkumas
                .Include(e => e.Sozlesme)
                    .ThenInclude(s => s.TuketimNoktasi) 
                .Include(e => e.Sozlesme)
                    .ThenInclude(s => s.Tarife)
                .Include(e => e.Sayac)
                .Where(e => e.DogrulamaDurumu == "ONAYLANDI")
                .ToListAsync();

            if (!okunmusEndeksler.Any())
                return BadRequest(new { message = "Onaylanan endeksler bulunamadı." });

            var yeniFaturalar = new List<Fatura>();
            string faturaPrefix = $"FAT-{DateTime.UtcNow:yyyyMM}-";

            int faturaSira = await _context.Faturas.CountAsync(f => f.FaturaTarihi.Year == DateTime.UtcNow.Year && f.FaturaTarihi.Month == DateTime.UtcNow.Month) + 1;

            foreach (var endeks in okunmusEndeksler)
            {
                // ⚡ Net tüketim hesabı
                decimal hamTuketim = (decimal)(endeks.YeniEndeks - endeks.OncekiEndeks!);
                if (hamTuketim <= 0) continue; 

                decimal carpan = endeks.Sayac?.Carpan ?? 1m;
                decimal gercekTuketimKwh = hamTuketim * carpan;

                var tarife = endeks.Sozlesme.Tarife;

                decimal enerjiBedeli = gercekTuketimKwh * tarife.GunduzBirimFiyat; 
                decimal dagitimBedeli = gercekTuketimKwh * tarife.DagitimBedeli;
                decimal hizmetBedeli = 15.50m; 

                decimal vergisizToplam = enerjiBedeli + dagitimBedeli;
                decimal vergiFon = vergisizToplam * (tarife.KdvOrani / 100m); 
                decimal toplamTutar = vergisizToplam + hizmetBedeli + vergiFon;

                var yeniFatura = new Fatura
                {
                    FaturaNo = $"{faturaPrefix}{faturaSira:D5}",
                    SozlesmeId = endeks.SozlesmeId.Value,
                    TekilKod = endeks.Sozlesme.TuketimNoktasi!.TekilKod, 
                    FaturaTipi = "DONEM",
                    Donem = endeks.Donem,
                    
                    // EF Core uyumu için türü senkron tutuyoruz
                    FaturaTarihi = DateOnly.FromDateTime(DateTime.UtcNow),
                    SonOdemeTarihi = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                    
                    OkumaId = endeks.OkumaId,
                    IlkEndeks = endeks.OncekiEndeks,
                    SonEndeks = endeks.YeniEndeks,
                    TuketimKwh = Math.Round(gercekTuketimKwh, 2),
                    
                    ReaktifEnduktif = 0m,
                    ReaktifKapasitif = 0m,
                    Carpan = carpan,
                    
                    EnerjiBedeli = Math.Round(enerjiBedeli, 2),
                    DagitimBedeli = Math.Round(dagitimBedeli, 2),
                    HizmetBedeli = hizmetBedeli,
                    KesmeBaglamaBedeli = 0m, 
                    VergiFonToplam = Math.Round(vergiFon, 2),
                    ToplamTutar = Math.Round(toplamTutar, 2),

                    Durum = "HESAPLANDI",
                    Status = "AKTIF",
                    CreatedAt = DateTime.UtcNow,

                    FaturaKalemis = new List<FaturaKalemi>
                    {
                        new FaturaKalemi { KalemTipi = "ENERJI_BEDELI", Miktar = gercekTuketimKwh, BirimFiyat = tarife.GunduzBirimFiyat, Tutar = Math.Round(enerjiBedeli, 2), Aciklama = "Aktif Enerji Bedeli" },
                        new FaturaKalemi { KalemTipi = "DAGITIM_BEDELI", Miktar = gercekTuketimKwh, BirimFiyat = tarife.DagitimBedeli, Tutar = Math.Round(dagitimBedeli, 2), Aciklama = "Dağıtım Sistemi Kullanım Bedeli" },
                        new FaturaKalemi { KalemTipi = "HIZMET_BEDELI", Miktar = 1, BirimFiyat = hizmetBedeli, Tutar = hizmetBedeli, Aciklama = "Sabit Hizmet Bedeli" },
                        new FaturaKalemi { KalemTipi = "VERGI_FON", Miktar = 1, BirimFiyat = Math.Round(vergiFon, 2), Tutar = Math.Round(vergiFon, 2), Aciklama = "KDV ve Diğer Fonlar" }
                    }
                };

                yeniFaturalar.Add(yeniFatura);
                endeks.DogrulamaDurumu = "TAHAKKUKA_AKTARILDI";
                faturaSira++;
            }

            _context.Faturas.AddRange(yeniFaturalar);
            
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new 
            { 
                message = $"{yeniFaturalar.Count} adet fatura başarıyla oluşturuldu ve onay bekliyor.",
                beklenenCiro = yeniFaturalar.Sum(f => f.ToplamTutar) + " TL"
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "Fatura oluşturma sırasında bir hata oluştu.", error = ex.InnerException?.Message ?? ex.Message });
        }
    }
}
}
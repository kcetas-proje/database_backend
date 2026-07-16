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

        [HttpPost]
        public async Task<IActionResult> YeniFaturaEkle([FromBody] FaturaCreateDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try 
            {
                // 1. Sözleşme ve Sayaç Bulma
                var sozlesme = await _context.Sozlesmelers
                    .Include(s => s.TuketimNoktasi)
                    .ThenInclude(t => t.Sayaclars)
                    .FirstOrDefaultAsync(s => s.SozlesmeId == dto.SozlesmeId);
                    
                if (sozlesme == null) 
                    return NotFound("Sözleşme bulunamadı");
                    
                var sayac = sozlesme.TuketimNoktasi?.Sayaclars?.FirstOrDefault();
                if (sayac == null) 
                    return BadRequest("Bu sözleşmeye bağlı bir sayaç bulunamadı.");

                // 2. Endeks Okuma Kaydı
                var yeniOkuma = new EndeksOkuma
                {
                    SayacId = sayac.SayacId,
                    IsEmriId = dto.IsEmriId,
                    SozlesmeId = dto.SozlesmeId,
                    OkumaTipi = "NORMAL",
                    OkumaKaynagi = "MOBIL",
                    OncekiEndeks = dto.IlkEndeks,
                    YeniEndeks = dto.SonEndeks,
                    Donem = dto.Donem,
                    OkumaZamani = dto.OkumaZamani != default ? dto.OkumaZamani : DateTime.UtcNow,
                    KullaniciId = dto.KullaniciId,
                    DogrulamaDurumu = "ONAYLANDI",
                    AnomaliMi = false,
                    Status = "AKTIF",
                    CreatedAt = DateTime.UtcNow
                };
                
                _context.EndeksOkumas.Add(yeniOkuma);
                await _context.SaveChangesAsync(); // OkumaId almak için

                // 3. Fatura Kaydı
                string rasgeleFaturaNo = "FAT" + DateTime.Now.ToString("yyyyMMddHHmmss");
                string rasgeleTekilKod = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();

                var yeniFatura = new Fatura
                {
                    SozlesmeId = dto.SozlesmeId,
                    OkumaId = yeniOkuma.OkumaId,
                    FaturaNo = rasgeleFaturaNo,
                    TekilKod = rasgeleTekilKod,
                    FaturaTipi = string.IsNullOrEmpty(dto.FaturaTipi) ? "DONEM" : dto.FaturaTipi,
                    Donem = string.IsNullOrEmpty(dto.Donem) ? DateTime.Now.ToString("yyyy-MM") : dto.Donem,
                    FaturaTarihi = DateOnly.FromDateTime(DateTime.UtcNow),
                    SonOdemeTarihi = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                    IlkEndeks = dto.IlkEndeks,
                    SonEndeks = dto.SonEndeks,
                    TuketimKwh = dto.TuketimKwh,
                    ReaktifEnduktif = dto.ReaktifEnduktif,
                    ReaktifKapasitif = dto.ReaktifKapasitif,
                    ToplamTutar = dto.ToplamTutar,
                    EnerjiBedeli = dto.ToplamTutar * 0.50m,
                    DagitimBedeli = dto.ToplamTutar * 0.30m,
                    VergiFonToplam = dto.ToplamTutar * 0.20m,
                    HizmetBedeli = 0m,
                    KesmeBaglamaBedeli = 0m,
                    Carpan = 1m,
                    Durum = string.IsNullOrEmpty(dto.Durum) ? "ODENMEDI" : dto.Durum, 
                    Status = "AKTIF",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Faturas.Add(yeniFatura);

                // İş emri varsa tamamla
                if (dto.IsEmriId.HasValue) 
                {
                    var isEmri = await _context.IsEmirleris.FindAsync(dto.IsEmriId.Value);
                    if (isEmri != null) 
                    {
                        isEmri.Durum = "TAMAMLANDI";
                        isEmri.UpdatedAt = DateTime.UtcNow;
                    }
                }

                // 4. Outbox Kaydı
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
                await transaction.CommitAsync();

                return Ok(yeniFatura);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "";
                return StatusCode(500, new { message = "Fatura oluşturulurken hata oluştu", details = ex.Message, inner = innerMessage });
            }
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

            bool faturaZatenVarMi = await _context.Faturas
                .AnyAsync(f => f.SozlesmeId == sozlesme.SozlesmeId 
                            && f.Donem == donemBilgisi 
                            && f.FaturaTipi == "DONEM" 
                            && f.Status == "AKTIF" 
                            && f.Durum != "IPTAL");

            if (faturaZatenVarMi)
            {
                return BadRequest(new { 
                    message = "Bu sözleşme için bu döneme ait aktif bir fatura zaten mevcut. Lütfen önce mevcut faturayı iptal edin veya onaylayın." 
                });
            }

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

    [HttpPost("{faturaId}/onayla")]
    public async Task<IActionResult> FaturaOnayla(long faturaId)
    {
        var fatura = await _context.Faturas.FindAsync(faturaId);
        
        if (fatura == null) 
            return NotFound(new { message = "Fatura bulunamadı!" });

        if (fatura.Durum != "HESAPLANDI")
            return BadRequest(new { message = $"Fatura şu an '{fatura.Durum}' durumunda. Sadece HESAPLANDI olanlar onaylanabilir." });

        // Statüyü güncelliyoruz
        fatura.Durum = "ONAYLANDI";
        fatura.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Fatura onaylandı!", faturaNo = fatura.FaturaNo });
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
        int faturaSira = 1;

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
                FaturaNo = $"{faturaPrefix}{faturaSira:D4}",
                SozlesmeId = endeks.SozlesmeId.Value,
                TekilKod = endeks.Sozlesme.TuketimNoktasi!.TekilKod, 
                FaturaTipi = "DONEM",
                Donem = endeks.Donem,

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
                CreatedAt = DateTime.UtcNow
            };

            yeniFaturalar.Add(yeniFatura);

            endeks.DogrulamaDurumu = "TAHAKKUKA_AKTARILDI";
            
            faturaSira++;
        }
        _context.Faturas.AddRange(yeniFaturalar);
        await _context.SaveChangesAsync();

        return Ok(new 
        { 
            message = $"{yeniFaturalar.Count} adet fatura başarıyla oluşturuldu.",
            beklenenCiro = yeniFaturalar.Sum(f => f.ToplamTutar) + " TL"
        });
    }
    }
}
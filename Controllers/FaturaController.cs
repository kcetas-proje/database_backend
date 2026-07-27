using KcetasAboneApi.Models;
using KcetasAboneApi.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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

            return Ok(new
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize,
                Data = faturalar
            });
        }

        [HttpPost("FaturaKes")]
        public async Task<IActionResult> FaturaKes([FromBody] FaturaKesRequestDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var okuma = await _context.EndeksOkumas.FirstOrDefaultAsync(o => o.OkumaId == request.OkumaId);
                if (okuma == null) return NotFound(new { message = "Okuma kaydı bulunamadı." });
                if (okuma.DogrulamaDurumu != DogrulamaDurumu.ONAYLANDI) return BadRequest(new { message = "Onaylanmamış okumadan fatura kesilemez!" });

                bool faturaVarMi = await _context.Faturas.AnyAsync(f => f.OkumaId == request.OkumaId && f.Status == "AKTIF");
                if (faturaVarMi) return BadRequest(new { message = "Bu okuma için zaten fatura kesilmiş boss!" });

                var sozlesme = await _context.Sozlesmelers.FirstOrDefaultAsync(s => s.SozlesmeId == request.SozlesmeId);
                if (sozlesme == null) return NotFound(new { message = "Sözleşme bulunamadı." });

                decimal carpan = 1.000m; 
                decimal oncekiEndeksDegeri = okuma.OncekiEndeks ?? 0m; // Null güvenliği

                if (oncekiEndeksDegeri > okuma.YeniEndeks)
                {
                    return BadRequest(new { message = "İlk endeks, son endeksten büyük olamaz." });
                }

                decimal tuketimKwh = (okuma.YeniEndeks - oncekiEndeksDegeri) * carpan;
                if (tuketimKwh < 0) tuketimKwh = 0; 

                decimal enerjiBirimFiyat = 2.50m;
                decimal dagitimBirimFiyat = 1.20m;
                
                decimal enerjiBedeli = tuketimKwh * enerjiBirimFiyat;
                decimal dagitimBedeli = tuketimKwh * dagitimBirimFiyat;
                decimal vergiFonToplam = enerjiBedeli * 0.20m; 
                decimal toplamTutar = enerjiBedeli + dagitimBedeli + vergiFonToplam;

                var fatura = new Fatura
                {
                    FaturaNo = $"FAT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0,6).ToUpper()}",
                    SozlesmeId = request.SozlesmeId,
                    OkumaId = okuma.OkumaId,
                    Donem = okuma.Donem,
                    FaturaTarihi = DateOnly.FromDateTime(DateTime.UtcNow), // Model uyumu
                    SonOdemeTarihi = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), // Model uyumu
                    IlkEndeks = oncekiEndeksDegeri,
                    SonEndeks = okuma.YeniEndeks,
                    TuketimKwh = tuketimKwh,
                    Carpan = carpan,
                    EnerjiBedeli = enerjiBedeli,
                    DagitimBedeli = dagitimBedeli,
                    VergiFonToplam = vergiFonToplam,
                    ToplamTutar = toplamTutar,
                    Durum = FaturaDurumu.ONAYLANDI, 
                    Status = "AKTIF",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Faturas.Add(fatura);
                await _context.SaveChangesAsync(); 

                var kalemler = new List<FaturaKalemi>
                {
                    new FaturaKalemi { FaturaId = fatura.FaturaId, KalemTipi = KalemTipi.ENERJI, Miktar = tuketimKwh, BirimFiyat = enerjiBirimFiyat, Tutar = enerjiBedeli, Aciklama = "Aktif Enerji Bedeli" },
                    new FaturaKalemi { FaturaId = fatura.FaturaId, KalemTipi = KalemTipi.DAGITIM, Miktar = tuketimKwh, BirimFiyat = dagitimBirimFiyat, Tutar = dagitimBedeli, Aciklama = "Dağıtım Sistemi Kullanım Bedeli" },
                    new FaturaKalemi { FaturaId = fatura.FaturaId, KalemTipi = KalemTipi.VERGI_FON, Miktar = 1, BirimFiyat = vergiFonToplam, Tutar = vergiFonToplam, Aciklama = "KDV ve Diğer Fonlar" }
                };
                
                _context.FaturaKalemis.AddRange(kalemler);

                _context.MevcutKullaniciId = request.IslemYapanKullaniciId; 

                var outboxKargosu = new EntegrasyonOutbox
                {
                    FaturaId = fatura.FaturaId,
                    HedefSistem = HedefSistem.GIB_EFATURA,
                    Durum = OutboxDurumu.BEKLIYOR,
                    IdempotencyKey = Guid.NewGuid().ToString(),
                    Payload = JsonSerializer.Serialize(new { faturaNo = fatura.FaturaNo, tutar = fatura.ToplamTutar, vergi = fatura.VergiFonToplam }),
                    CreatedAt = DateTime.UtcNow
                };

                _context.EntegrasyonOutboxes.Add(outboxKargosu);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Fatura kesildi", faturaNo = fatura.FaturaNo, tutar = fatura.ToplamTutar });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Hata oluştu", error = ex.InnerException?.Message ?? ex.Message });
            }
        }


        [HttpPost("{faturaId}/tahsil-et")]
        public async Task<IActionResult> FaturaTahsilEt(long faturaId)
        {

            var fatura = await _context.Faturas.FindAsync(faturaId);
            if (fatura == null) return NotFound(new { message = "Fatura bulunamadı!" });

            if (fatura.Durum == FaturaDurumu.ODENDI) 
                return BadRequest(new { message = "Bu fatura zaten daha önce tahsil edilmiş!" });
            
            if (fatura.Durum != FaturaDurumu.ODENMEDI && fatura.Durum != FaturaDurumu.GONDERILDI)
                return BadRequest(new { message = $"Sadece ODENMEDI veya GONDERILDI durumundaki faturalar tahsil edilebilir. Mevcut durum: {fatura.Durum.ToString()}" });

            // 3. Durumu güncelle
            fatura.Durum = FaturaDurumu.ODENDI;
            fatura.UpdatedAt = DateTime.UtcNow;

            // 4. Kaydet
            await _context.SaveChangesAsync();

            return Ok(new { 
                message = $"{fatura.FaturaNo} numaralı fatura başarıyla tahsil edildi!", 
                tarih = DateTime.UtcNow 
            });
        }

        [HttpPost]
        public async Task<IActionResult> YeniFaturaEkle([FromBody] FaturaCreateDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try 
            {

                if (dto.IlkEndeks > dto.SonEndeks)
                {
                    return BadRequest(new { message = "İlk endeks, son endeksten büyük olamaz." });
                }

                var sozlesme = await _context.Sozlesmelers
                    .Include(s => s.TuketimNoktasi)
                        .ThenInclude(t => t.Sayaclars)
                    .FirstOrDefaultAsync(s => s.SozlesmeId == dto.SozlesmeId);
                    
                if (sozlesme == null) 
                    return NotFound("Sözleşme bulunamadı.");
                    
                var sayac = sozlesme.TuketimNoktasi?.Sayaclars?.FirstOrDefault();
                if (sayac == null) 
                    return BadRequest("Bu sözleşmeye bağlı aktif sayaç yok!");

                var yeniOkuma = new EndeksOkuma
                {
                    SayacId = sayac.SayacId,
                    IsEmriId = dto.IsEmriId,
                    SozlesmeId = dto.SozlesmeId,
                    OkumaTipi = OkumaTipi.RUTIN_DONEM,
                    OkumaKaynagi = OkumaKaynagi.OSOS,
                    OncekiEndeks = dto.IlkEndeks,
                    YeniEndeks = dto.SonEndeks,
                    Donem = dto.Donem,
                    OkumaZamani = dto.OkumaZamani != default ? dto.OkumaZamani : DateTime.UtcNow,
                    KullaniciId = dto.KullaniciId,
                    DogrulamaDurumu = DogrulamaDurumu.ONAYLANDI,
                    AnomaliMi = false,
                    Status = "AKTIF",
                    CreatedAt = DateTime.UtcNow
                };
                
                _context.EndeksOkumas.Add(yeniOkuma);
                await _context.SaveChangesAsync(); 

                string rasgeleFaturaNo = "FAT" + DateTime.Now.ToString("yyyyMMddHHmmss");
                string rasgeleTekilKod = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();

                decimal enerjiBedeli = dto.ToplamTutar * 0.50m;
                decimal dagitimBedeli = dto.ToplamTutar * 0.30m;
                decimal vergiFonBedeli = dto.ToplamTutar * 0.20m;

                var yeniFatura = new Fatura
                {
                    SozlesmeId = dto.SozlesmeId,
                    OkumaId = yeniOkuma.OkumaId,
                    FaturaNo = rasgeleFaturaNo,
                    TekilKod = rasgeleTekilKod,
                    FaturaTipi = dto.FaturaTipi == 0 ? FaturaTipi.DONEM : dto.FaturaTipi,
                    Donem = string.IsNullOrEmpty(dto.Donem) ? DateTime.Now.ToString("yyyy-MM") : dto.Donem,
                    FaturaTarihi = DateOnly.FromDateTime(DateTime.UtcNow),
                    SonOdemeTarihi = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                    IlkEndeks = dto.IlkEndeks,
                    SonEndeks = dto.SonEndeks,
                    TuketimKwh = dto.TuketimKwh,
                    ReaktifEnduktif = dto.ReaktifEnduktif,
                    ReaktifKapasitif = dto.ReaktifKapasitif,
                    ToplamTutar = dto.ToplamTutar,
                    EnerjiBedeli = enerjiBedeli,
                    DagitimBedeli = dagitimBedeli,
                    VergiFonToplam = vergiFonBedeli,
                    HizmetBedeli = 0m,
                    KesmeBaglamaBedeli = 0m,
                    Carpan = 1m,
                    Durum = dto.Durum == 0 ? FaturaDurumu.ODENMEDI : dto.Durum, 
                    Status = "AKTIF",
                    CreatedAt = DateTime.UtcNow,

                    FaturaKalemis = new List<FaturaKalemi>
                    {
                        new FaturaKalemi { KalemTipi = KalemTipi.ENERJI, Miktar = dto.TuketimKwh, BirimFiyat = Math.Round(enerjiBedeli / (dto.TuketimKwh > 0 ? dto.TuketimKwh : 1), 4), Tutar = enerjiBedeli, Aciklama = "Aktif Enerji Bedeli" },
                        new FaturaKalemi { KalemTipi = KalemTipi.DAGITIM, Miktar = dto.TuketimKwh, BirimFiyat = Math.Round(dagitimBedeli / (dto.TuketimKwh > 0 ? dto.TuketimKwh : 1), 4), Tutar = dagitimBedeli, Aciklama = "Dağıtım Bedeli" },
                        new FaturaKalemi { KalemTipi = KalemTipi.VERGI_FON, Miktar = 1, BirimFiyat = vergiFonBedeli, Tutar = vergiFonBedeli, Aciklama = "Yasal Vergiler ve Fonlar" }
                    }
                };

                _context.Faturas.Add(yeniFatura);

                if (dto.IsEmriId.HasValue) 
                {
                    var isEmri = await _context.IsEmirleris.FindAsync(dto.IsEmriId.Value);
                    if (isEmri != null) 
                    {
                        isEmri.Durum = IsEmriDurumu.TAMAMLANDI;
                        isEmri.UpdatedAt = DateTime.UtcNow;
                    }
                }

                _context.MevcutKullaniciId = dto.KullaniciId;

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
                    HedefSistem = HedefSistem.GIB_EFATURA,
                    IdempotencyKey = Guid.NewGuid().ToString(), 
                    Payload = JsonSerializer.Serialize(payloadData),
                    Durum = OutboxDurumu.BEKLIYOR,
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
                return StatusCode(500, new { message = "Fatura oluştururken bir hata oluştu.", details = ex.Message, inner = innerMessage });
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
            if (dbFatura == null) return NotFound("Böyle bir fatura bulunamadı.");

            dbFatura.Status = IsEmriDurumu.IPTAL.ToString();
            dbFatura.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { mesaj = $"{dbFatura.FaturaNo} numaralı fatura başarıyla iptal duruma alındı." });
        }

    [HttpPost("{faturaId}/onayla")]
    public async Task<IActionResult> FaturaOnayla(long faturaId)
    {
        var fatura = await _context.Faturas.FindAsync(faturaId);
        if (fatura == null) return NotFound(new { message = "Fatura bulunamadı!" });

        if (fatura.Durum != FaturaDurumu.HESAPLANDI) return BadRequest(new { message = $"Sadece HESAPLANDI olanlar onaylanabilir." });

        fatura.Durum = FaturaDurumu.ONAYLANDI;
        fatura.UpdatedAt = DateTime.UtcNow;


            var outboxKargosu = new EntegrasyonOutbox
            {
                FaturaId = fatura.FaturaId,
                HedefSistem = HedefSistem.GIB_EFATURA,
                Durum = OutboxDurumu.BEKLIYOR,
                IdempotencyKey = Guid.NewGuid().ToString(),
                Payload = JsonSerializer.Serialize(new { faturaNo = fatura.FaturaNo, tutar = fatura.ToplamTutar }),
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
            if (fatura == null) return NotFound(new { message = "Fatura bulunamadı!" });

            if (fatura.Durum == FaturaDurumu.GONDERILDI || fatura.Durum == FaturaDurumu.IPTAL)
                return BadRequest(new { message = "Fatura zaten gönderilmiş veya iptal edilmiş!" });

            fatura.Durum = FaturaDurumu.IPTAL;
            fatura.Status = BaglantiDurumu.PASIF.ToString(); 
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
                    .Include(e => e.Sozlesme).ThenInclude(s => s.TuketimNoktasi) 
                    .Include(e => e.Sozlesme).ThenInclude(s => s.Tarife)
                    .Include(e => e.Sayac)
                    .Where(e => e.DogrulamaDurumu == DogrulamaDurumu.ONAYLANDI)
                    .ToListAsync();

                if (!okunmusEndeksler.Any())
                    return BadRequest(new { message = "Onaylanan endeksler bulunamadı." });

                var yeniFaturalar = new List<Fatura>();
                string faturaPrefix = $"FAT-{DateTime.UtcNow:yyyyMM}-";
                
                int faturaSira = await _context.Faturas.CountAsync(f => f.FaturaTarihi.Year == DateTime.UtcNow.Year && f.FaturaTarihi.Month == DateTime.UtcNow.Month) + 1;

                foreach (var endeks in okunmusEndeksler)
                {
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
                        FaturaTipi = FaturaTipi.DONEM,
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
                        Durum = FaturaDurumu.HESAPLANDI,
                        Status = "AKTIF",
                        CreatedAt = DateTime.UtcNow,

                        FaturaKalemis = new List<FaturaKalemi>
                        {
                            new FaturaKalemi { KalemTipi = KalemTipi.ENERJI, Miktar = gercekTuketimKwh, BirimFiyat = tarife.GunduzBirimFiyat, Tutar = Math.Round(enerjiBedeli, 2), Aciklama = "Aktif Enerji Bedeli" },
                            new FaturaKalemi { KalemTipi = KalemTipi.DAGITIM, Miktar = gercekTuketimKwh, BirimFiyat = tarife.DagitimBedeli, Tutar = Math.Round(dagitimBedeli, 2), Aciklama = "Dağıtım Sistemi Kullanım Bedeli" },
                            new FaturaKalemi { KalemTipi = KalemTipi.HIZMET, Miktar = 1, BirimFiyat = hizmetBedeli, Tutar = hizmetBedeli, Aciklama = "Sabit Hizmet Bedeli" },
                            new FaturaKalemi { KalemTipi = KalemTipi.VERGI_FON, Miktar = 1, BirimFiyat = Math.Round(vergiFon, 2), Tutar = Math.Round(vergiFon, 2), Aciklama = "KDV ve Diğer Fonlar" }
                        }
                    };

                    yeniFaturalar.Add(yeniFatura);
                    endeks.DogrulamaDurumu = DogrulamaDurumu.TAHAKKUKA_AKTARILDI;
                    faturaSira++;
                }

                _context.Faturas.AddRange(yeniFaturalar);
                
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new 
                { 
                    message = $"{yeniFaturalar.Count} adet fatura başarıyla oluşturuldu.",
                    beklenenCiro = yeniFaturalar.Sum(f => f.ToplamTutar) + " TL"
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Hata oluştu.", error = ex.InnerException?.Message ?? ex.Message });
            }
        }
    }
}
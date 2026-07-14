using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KcetasAboneApi.Models;
using Microsoft.AspNetCore.Authorization;
using KcetasAboneApi.Models.Dtos;
using Bogus;

namespace KcetasAboneApi.Controllers;

//[Authorize(Roles = "1, 5")]
[Route("api/[controller]")]
[ApiController]
public class IsEmirleriController : ControllerBase
{
    private readonly AppDbContext _context; 

    public IsEmirleriController(AppDbContext context) 
    {
        _context = context;
    }

    // GET: api/IsEmirleri
    [HttpGet]
    public async Task<IActionResult> GetIsEmirleri([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var totalCount = await _context.IsEmirleris.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var isEmirleri = await _context.IsEmirleris
            .Include(i => i.Sayac)
            .Include(i => i.TuketimNoktasi)
            .OrderBy(i => i.IsEmriId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var response = new
        {
            TotalCount = totalCount,
            TotalPages = totalPages,
            CurrentPage = page,
            PageSize = pageSize,
            Data = isEmirleri
        };

        return Ok(response);
    }

    // GET: api/IsEmirleri/5
    [HttpGet("{id}")]
    public async Task<ActionResult<IsEmirleri>> GetIsEmri(long id)
    {
        var isEmri = await _context.IsEmirleris
            .Include(i => i.Sayac)
            .Include(i => i.TuketimNoktasi)
            .FirstOrDefaultAsync(i => i.IsEmriId == id);

        if (isEmri == null)
        {
            return NotFound(new { message = "İş emri bulunamadı." });
        }

        return isEmri;
    }

    // POST: api/IsEmirleri
    [HttpPost]
    public async Task<ActionResult<IsEmirleri>> PostIsEmri(IsEmriCreateDto dto)
    {
        if (!await _context.TuketimNoktasis.AnyAsync(x => x.TuketimNoktasiId == dto.TuketimNoktasiId))
        {
            return BadRequest(new { message = "Geçersiz tüketim noktası." });
        }

        long? finalSayacId = dto.SayacId;

        if (finalSayacId != null)
        {
            if (!await _context.Sayaclars.AnyAsync(x => x.SayacId == finalSayacId))
                return BadRequest(new { message = "Sayaç bulunamadı." });
        }
        else if (dto.Tip != "YENI_BAGLANTI")
        {
            // Eğer sayaç ID gönderilmemişse ve yeni bağlantı değilse, o tüketim noktasındaki takılı sayacı otomatik bul.
            var takiliSayac = await _context.Sayaclars
                .FirstOrDefaultAsync(s => s.TuketimNoktasiId == dto.TuketimNoktasiId && s.Durum == "TAKILI");
            
            if (takiliSayac != null)
            {
                finalSayacId = takiliSayac.SayacId;
            }
        }

        if (dto.AtananKullaniciId != null)
        {
            if (!await _context.Kullanicilars.AnyAsync(x => x.KullaniciId == dto.AtananKullaniciId))
                return BadRequest(new { message = "Kullanıcı bulunamadı." });
        }

        string prefix = $"IE-{DateTime.UtcNow:yyyyMM}-";

        var sonIsEmri = await _context.IsEmirleris
        .Where(x => x.IsEmriNo.StartsWith(prefix))
        .OrderByDescending(x => x.IsEmriNo)
        .FirstOrDefaultAsync();

        int yeniSira = 1;
        if (sonIsEmri != null)
        {
            string sonSiraStr = sonIsEmri.IsEmriNo.Substring(sonIsEmri.IsEmriNo.Length - 4);
            if (int.TryParse(sonSiraStr, out int sonSiraInt))
            {
                yeniSira = sonSiraInt + 1;
            }
        }

        string yeniIsEmriNo = $"{prefix}{yeniSira:D4}";

        var yeniIsEmri = new IsEmirleri
        {
            IsEmriNo = yeniIsEmriNo,
            TuketimNoktasiId = dto.TuketimNoktasiId,
            SayacId = finalSayacId,
            AtananKullaniciId = dto.AtananKullaniciId,
            Tip = dto.Tip,
            Oncelik = dto.Oncelik,
            Durum = dto.Durum,
            CreatedAt = DateTime.UtcNow
            
        };

        _context.IsEmirleris.Add(yeniIsEmri);
        await _context.SaveChangesAsync();

        return Ok(new { message = "İş emri oluşturuldu!", data = yeniIsEmri });
    }

    // PUT: api/IsEmirleri/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutIsEmri(long id, IsEmirleri isEmri)
    {
        if (id != isEmri.IsEmriId)
        {
            return BadRequest(new
            {
                message = "İş Emri Id uyuşmuyor."
            });
        }

        var mevcut = await _context.IsEmirleris.FindAsync(id);

        if (mevcut == null)
        {
            return NotFound(new
            {
                message = "İş emri bulunamadı."
            });
        }

        mevcut.TuketimNoktasiId = isEmri.TuketimNoktasiId;
        mevcut.SayacId = isEmri.SayacId;
        mevcut.Tip = isEmri.Tip;
        mevcut.Oncelik = isEmri.Oncelik;
        mevcut.PlanlananTarih = isEmri.PlanlananTarih;
        mevcut.AtananKullaniciId = isEmri.AtananKullaniciId;
        mevcut.Durum = isEmri.Durum;
        mevcut.SahaSonucu = isEmri.SahaSonucu;
        mevcut.Gerekce = isEmri.Gerekce;
        mevcut.MuhurNo = isEmri.MuhurNo;
        mevcut.TutanakNo = isEmri.TutanakNo;
        mevcut.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/IsEmirleri/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteIsEmri(long id)
    {
        var isEmri = await _context.IsEmirleris.FindAsync(id);

        if (isEmri == null)
        {
            return NotFound(new
            {
                message = "İş emri bulunamadı."
            });
        }

        // Endeks okuma kontrolü
        bool kullaniliyor = await _context.EndeksOkumas
            .AnyAsync(x => x.IsEmriId == id);

        if (kullaniliyor)
        {
            return BadRequest(new
            {
                message = "Bu iş emrine bağlı endeks okuma bulunduğu için silinemez."
            });
        }

        _context.IsEmirleris.Remove(isEmri);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("tamamla/{isEmriId}/sayac/{sayacId}")]
    public async Task<IActionResult> IsEmriTamamla(long isEmriId, long sayacId)
    {
        var isEmri = await _context.IsEmirleris.FindAsync(isEmriId);
        if (isEmri == null || isEmri.Durum != "ACIK")
            return BadRequest(new { message = "Böyle bir açık iş emri yok." });

        var sayac = await _context.Sayaclars.FindAsync(sayacId);
        if (sayac == null || sayac.Durum != "DEPODA")
            return BadRequest(new { message = "Bu sayaç depoda değil." });

        isEmri.SayacId = sayac.SayacId; 
        isEmri.Durum = "TAMAMLANDI";
        
        isEmri.UpdatedAt = DateTime.UtcNow; 

        sayac.TuketimNoktasiId = isEmri.TuketimNoktasiId;
        sayac.Durum = "TAKILI";
        sayac.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new 
        { 
            message = $"İş emri {isEmri.IsEmriNo} başarıyla tamamlandı ve sayaç {sayac.SeriNo} takıldı!", 
        });
    }

    [HttpPost("toplu-yeni-baglanti-onayla")]
    public async Task<IActionResult> TopluYeniBaglantiOnayla()
    {

        var acikIsEmirleri = await _context.IsEmirleris
            .Where(i => i.Tip == "YENI_BAGLANTI" && i.Durum == "ACIK")
            .ToListAsync();

        var depodakiSayaclar = await _context.Sayaclars
            .Where(s => s.Durum == "DEPODA")
            .ToListAsync();

        int islemKapasitesi = Math.Min(acikIsEmirleri.Count, depodakiSayaclar.Count);

        if (islemKapasitesi == 0)
            return BadRequest(new { message = "Sahada açık iş emri yok ya da depoda sayaç kalmamış!" });

        for (int i = 0; i < islemKapasitesi; i++)
        {
            var isEmri = acikIsEmirleri[i];
            var sayac = depodakiSayaclar[i];

            isEmri.SayacId = sayac.SayacId; 
            isEmri.Durum = "TAMAMLANDI";

            sayac.TuketimNoktasiId = isEmri.TuketimNoktasiId;
            sayac.Durum = "TAKILI";
        }

        await _context.SaveChangesAsync();

        return Ok(new 
        { 
            message = $"{islemKapasitesi} adet YENI_BAGLANTI iş emri başarıyla tamamlandı ve sayaçlar mekanlara takıldı!",
            onaylananSayi = islemKapasitesi
        });
    }

    [HttpPost("generate-random-is-emirleri")]
    public async Task<IActionResult> GenerateRandomIsEmirleri()
    {

        var takiliSayaclar = await _context.Sayaclars
            .Where(s => s.Durum == "TAKILI" && s.TuketimNoktasiId != null)
            .ToListAsync();

        if (!takiliSayaclar.Any())
            return BadRequest(new { message = "Takılı sayaç bulunamadı!" });

        string isEmriPrefix = $"IE-{DateTime.UtcNow:yyyyMM}-";
        int isEmriSira = 1;
        var sonIsEmri = await _context.IsEmirleris
            .Where(x => x.IsEmriNo.StartsWith(isEmriPrefix))
            .OrderByDescending(x => x.IsEmriNo)
            .FirstOrDefaultAsync();

        if (sonIsEmri != null && int.TryParse(sonIsEmri.IsEmriNo.Substring(sonIsEmri.IsEmriNo.Length - 4), out int iSira))
            isEmriSira = iSira + 1;

        var isEmriTipleri = new[] 
        { 
            "DEGISTIRME", "SOKME", "KESME", "ACMA", 
            "ENDEKS_OKUMA", "SAYAC_ARIZA", "MUHURLEME", "KESIF_INCELEME" 
        };
        
        var oncelikler = new[] { "DUSUK", "NORMAL", "YUKSEK", "ACIL" };
        var sahteIsEmirleri = new List<IsEmirleri>();
        var random = new Random();

        for (int j = 0; j < 30; j++)
        {
            var secilenSayac = takiliSayaclar[random.Next(takiliSayaclar.Count)];
            string secilenTip = isEmriTipleri[random.Next(isEmriTipleri.Length)];

            var yeniIsEmri = new IsEmirleri
            {
                IsEmriNo = $"{isEmriPrefix}{(isEmriSira + j):D4}",
                TuketimNoktasiId = secilenSayac.TuketimNoktasiId.Value,
                SayacId = secilenSayac.SayacId, 
                Tip = secilenTip,

                Oncelik = (secilenTip == "SAYAC_ARIZA" || secilenTip == "KESME") ? "ACIL" : oncelikler[random.Next(oncelikler.Length)],
                
                Durum = "ACIK",
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 7)).ToUniversalTime(),
            };

            sahteIsEmirleri.Add(yeniIsEmri);
        }

        _context.IsEmirleris.AddRange(sahteIsEmirleri);
        await _context.SaveChangesAsync();

        return Ok(new 
        { 
            message = "30 adet sahte iş emri başarıyla oluşturuldu.", 
            eklenenSayi = sahteIsEmirleri.Count 
        });
    }
    [HttpPost("CompleteJob")]
public async Task<IActionResult> CompleteJob([FromBody] CompleteJobRequestDto request)
{
    using var transaction = await _context.Database.BeginTransactionAsync();
    
    try
    {

        var isEmri = await _context.IsEmirleris
            .Include(x => x.Sayac) 
            .FirstOrDefaultAsync(x => x.IsEmriId == request.JobId);

        if (isEmri == null) return NotFound(new { message = "Böyle bir iş emri bulunamadı." });
        if (isEmri.Durum == "TAMAMLANDI") return BadRequest(new { message = "Bu iş emri zaten tamamlanmış!" });

        isEmri.Durum = "TAMAMLANDI";
        isEmri.UpdatedAt = DateTime.UtcNow; 

        var sonOkuma = await _context.EndeksOkumas
            .Where(e => e.SayacId == isEmri.SayacId)
            .OrderByDescending(e => e.CreatedAt)
            .FirstOrDefaultAsync();

        string dinamikOkumaTipi = isEmri.Tip switch
        {
            "KESME" => "KESME_ENDEKSI",
            "DEGISTIRME" => "SAYAC_DEGISIM_OKUMASI",
            "SAYAC_ARIZA" => "SAYAC_DEGISIM_OKUMASI",
            "SOKME" => "SON_OKUMA",
            _ => sonOkuma == null ? "ILK_OKUMA" : "RUTIN_DONEM"
        };

        var yeniEndeks = new EndeksOkuma
        {
            SayacId = isEmri.SayacId.Value, 
            IsEmriId = isEmri.IsEmriId,
            
            OkumaTipi = dinamikOkumaTipi, 
            
            OkumaKaynagi = "MANUEL",
            OncekiEndeks = sonOkuma?.YeniEndeks ?? 0m,
            YeniEndeks = request.SonEndeks, 
            Donem = $"{DateTime.UtcNow:yyyy/MM}",
            OkumaZamani = DateTime.UtcNow,
            KullaniciId = request.IslemYapanKullaniciId,
            DogrulamaDurumu = "ONAYLANDI",
            AnomaliMi = false,
            Status = "AKTIF",
            CreatedAt = DateTime.UtcNow
        };

        _context.EndeksOkumas.Add(yeniEndeks);

        if (isEmri.Sayac != null && !string.IsNullOrEmpty(request.MuhurNo))
        {
            isEmri.Sayac.MuhurNo = request.MuhurNo;
            isEmri.Sayac.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return Ok(new { message = "İş emri başarıyla tamamlandı." });
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        return StatusCode(500, new { message = "İş emri tamamlanırken bir hata oluştu.", error = ex.InnerException?.Message ?? ex.Message });
    }
}
}

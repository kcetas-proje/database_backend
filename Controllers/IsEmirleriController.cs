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

    // GET: api/IsEmirleri/All
    [HttpGet("All")]
    public async Task<ActionResult<IEnumerable<IsEmirleri>>> GetAllIsEmirleri([FromQuery] bool includeCompleted = false)
    {
        // 🚀 GIGACHAD FIX: Başlangıçta tüm veritabanını query'e al, filtre koyma!
        var query = _context.IsEmirleris.AsQueryable();

        // 🛡️ Şalter kapalıysa (Mobilse) tamamlanmış olanları nuke'le (gizle).
        // Web'den includeCompleted=true gelirse buraya hiç girmez, her şeyi çeker!
        if (!includeCompleted)
        {
            query = query.Where(i => i.Durum != "TAMAMLANDI");
        }

        return await query
            .Include(i => i.Sayac)
            .Include(i => i.TuketimNoktasi)
            .OrderBy(i => i.IsEmriId)
            .ToListAsync();
    }

    // GET: api/IsEmirleri
    [HttpGet]
    public async Task<IActionResult> GetIsEmirleri([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] bool includeCompleted = false)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        // 🚀 GIGACHAD FIX: Başlangıçta hiçbir statüyü filtreleme!
        var query = _context.IsEmirleris.AsQueryable();

        // 🛡️ Web'den true gelene kadar tamamlanmışları gizle!
        if (!includeCompleted) 
        {
            query = query.Where(i => i.Durum != "TAMAMLANDI");
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var isEmirleri = await query
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

    [HttpGet("by-no/{isEmriNo}")]
public async Task<IActionResult> GetByIsEmriNo(string isEmriNo)
{

    string temizlenmisNo = isEmriNo.Trim();

    var isEmri = await _context.IsEmirleris
        .Include(x => x.Sayac)
        .Include(x => x.TuketimNoktasi)
        .FirstOrDefaultAsync(x => x.IsEmriNo == temizlenmisNo);

    if (isEmri == null)
    {
        return NotFound(new 
        { 
            message = $"Böyle bir iş emri bulunamadı: {temizlenmisNo}" 
        });
    }

    return Ok(isEmri);
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
                .FirstOrDefaultAsync(s => s.TuketimNoktasiId == dto.TuketimNoktasiId && (s.Durum == "TAKILI" || s.Durum == "AKTIF"));
            
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
            "ENDEKS_OKUMA", "SAYAC_ARIZA", "MUHURLEME", "KESIF_INCELEME", "ENERJI_ACMA" 
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

            if (isEmri == null) 
                return NotFound(new { message = "Böyle bir iş emri bulunamadı." });
                
            if (isEmri.Durum == "TAMAMLANDI") 
                return BadRequest(new { message = "Bu iş emri zaten tamamlanmış." });

            if (isEmri.SayacId == null || !isEmri.SayacId.HasValue)
            {
                return BadRequest(new 
                { 
                    message = "Bu iş emrine ait sayaç bulunamadı." 
                });
            }

            long sayacId = isEmri.SayacId.Value;
            
            // 1. İş Emrini Kapat ve Detayları Ekle
            isEmri.Durum = "TAMAMLANDI";
            if (!string.IsNullOrEmpty(request.SahaSonucu))
                isEmri.SahaSonucu = request.SahaSonucu;
            
            isEmri.UpdatedAt = DateTime.UtcNow;

            var sonOkuma = await _context.EndeksOkumas
                .Where(e => e.SayacId == sayacId)
                .OrderByDescending(e => e.CreatedAt)
                .FirstOrDefaultAsync();

            string dinamikOkumaTipi = isEmri.Tip switch
            {
                "KESME" => "KESME_ENDEKSI",
                "DEGISTIRME" => "SAYAC_DEGISIM_OKUMASI",
                "SAYAC_ARIZA" => "SAYAC_ARIZA_OKUMASI", 
                "MUHURLEME" => "MUHURLEME_ENDEKSI", 
                "SOKME" => "SON_OKUMA",
                "ENERJI_ACMA" => "ILK_OKUMA", 
                "ACMA" => "ILK_OKUMA",
                "YENI_BAGLANTI" => "ILK_OKUMA", 
                _ => sonOkuma == null ? "ILK_OKUMA" : "RUTIN_DONEM"
            };

            var yeniEndeks = new EndeksOkuma
            {
                SayacId = sayacId,
                IsEmriId = isEmri.IsEmriId,
                OkumaTipi = dinamikOkumaTipi,
                OkumaKaynagi = "MANUEL",
                OncekiEndeks = sonOkuma?.YeniEndeks ?? 0m,
                YeniEndeks = request.SonEndeks,

                GunduzEndeks = request.Gunduz,
                PuantEndeks = request.Puant,
                GeceEndeks = request.Gece,
                InduktifEndeks = request.Induktif,
                KapasitifEndeks = request.Kapasitif,
                Demand = request.Demand,


                Donem = $"{DateTime.UtcNow:yyyy/MM}",
                OkumaZamani = DateTime.UtcNow,
                KullaniciId = request.IslemYapanKullaniciId,
                DogrulamaDurumu = "ONAYLANDI",
                AnomaliMi = false,
                Status = "AKTIF",
                CreatedAt = DateTime.UtcNow
            };

            _context.EndeksOkumas.Add(yeniEndeks);

            if (isEmri.Sayac != null)
            {
                if (!string.IsNullOrEmpty(request.MuhurNo))
                    isEmri.Sayac.MuhurNo = request.MuhurNo;

                isEmri.Sayac.Durum = isEmri.Tip switch
                {
                    "SOKME" => "SOKULMUS",
                    "DEGISTIRME" => "SOKULMUS",
                    "SAYAC_ARIZA" => "ARIZALI",
                    _ => isEmri.Sayac.Durum 
                };

                isEmri.Sayac.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { message = "İş emri başarıyla tamamlandı." });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "İş emri tamamlanırken hata oluştu.", error = ex.InnerException?.Message ?? ex.Message });
        }
    }

    [HttpPost("EnerjiAcma")]
    public async Task<IActionResult> EnerjiAcma([FromBody] EnerjiAcmaDto dto)
    {
        var isEmri = await _context.IsEmirleris.FindAsync(dto.IsEmriId);
        
        if (isEmri == null)
            return NotFound(new { message = "İş emri bulunamadı." });

        if (isEmri.Durum == "TAMAMLANDI")
            return BadRequest(new { message = "Bu iş emri zaten tamamlanmış." });

        // İş Emri Durum Güncellemesi
        isEmri.Durum = "TAMAMLANDI";
        isEmri.SahaSonucu = "Enerji Açıldı - Nokta: " + dto.AcmaNoktasi;
        isEmri.MuhurNo = dto.MuhurNo;
        isEmri.Gerekce = dto.Aciklama;
        isEmri.UpdatedAt = DateTime.UtcNow;

        // Endeks Okuma Kaydı (Enerji açılırken alınan ilk endeks)
        if (isEmri.SayacId.HasValue && isEmri.TuketimNoktasiId > 0)
        {
            // İlgili sözleşmeyi bulalım (Tüketim noktasına bağlı aktif sözleşme)
            var sozlesme = await _context.Sozlesmelers
                .FirstOrDefaultAsync(s => s.TuketimNoktasiId == isEmri.TuketimNoktasiId && s.Durum == "AKTIF");

            var okuma = new EndeksOkuma
            {
                SayacId = isEmri.SayacId.Value,
                IsEmriId = isEmri.IsEmriId,
                SozlesmeId = sozlesme?.SozlesmeId,
                OkumaTipi = "ACILIS",
                OkumaKaynagi = "MOBIL",
                OncekiEndeks = 0,
                YeniEndeks = dto.Aktif,
                OkumaZamani = DateTime.UtcNow,
                DogrulamaDurumu = "ONAYLANDI",
                AnomaliMi = false,
                Status = "AKTIF",
                CreatedAt = DateTime.UtcNow
            };
            
            _context.EndeksOkumas.Add(okuma);
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "Enerji açma işlemi başarıyla kaydedildi.", isEmriNo = isEmri.IsEmriNo });
    }

    // 1. Sayaç Değişimi Operasyonu
    [HttpPost("SayacDegisimi")]
    public async Task<IActionResult> SayacDegisimi([FromBody] SayacDegisimiRequestDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            // 1. İş Emrini Bul
            var isEmri = await _context.IsEmirleris
                .FirstOrDefaultAsync(x => x.IsEmriId == dto.JobId);

            if (isEmri == null) 
                return NotFound(new { message = "Böyle bir iş emri bulunamadı." });
                
            if (isEmri.Durum == "TAMAMLANDI") 
                return BadRequest(new { message = "Bu iş emri zaten tamamlanmış!" });

            isEmri.Durum = "TAMAMLANDI";
            isEmri.SokulenSeriNo = dto.SokulenSeriNo;
            isEmri.YeniSeriNo = dto.YeniSeriNo;
            isEmri.YeniMuhurNo = dto.YeniMuhurNo;
            isEmri.DamgaYili = dto.DamgaYili;
            isEmri.AkimTrafosuSeriNo = dto.AkimTrafosuSeriNo;
            isEmri.AkimTrafosuMarka = dto.AkimTrafosuMarka;
            isEmri.GerilimTrafosuSeriNo = dto.GerilimTrafosuSeriNo;
            isEmri.GerilimTrafosuMarka = dto.GerilimTrafosuMarka;
            isEmri.SahaSonucu = "Sayaç değişimi sahada tamamlandı.";
            isEmri.AtananKullaniciId = dto.IslemYapanKullaniciId;
            isEmri.UpdatedAt = DateTime.UtcNow;

            var eskiSayac = await _context.Sayaclars.FirstOrDefaultAsync(s => s.SeriNo == dto.SokulenSeriNo);
            if (eskiSayac != null)
            {
                eskiSayac.Durum = "SOKULMUS";
                eskiSayac.TuketimNoktasiId = null;
                eskiSayac.UpdatedAt = DateTime.UtcNow;

                var eskiOkuma = new EndeksOkuma
                {
                    SayacId = eskiSayac.SayacId,
                    IsEmriId = isEmri.IsEmriId,
                    OkumaTipi = "SAYAC_DEGISIM_OKUMASI",
                    OkumaKaynagi = "MANUEL",
                    YeniEndeks = dto.SokulenAktif, 
                    OkumaZamani = DateTime.UtcNow,
                    KullaniciId = dto.IslemYapanKullaniciId,
                    DogrulamaDurumu = "ONAYLANDI",
                    Status = "AKTIF",
                    CreatedAt = DateTime.UtcNow
                };
                _context.EndeksOkumas.Add(eskiOkuma);
            }

        // 4. Yeni Sayacı Mekana Tak
            var yeniSayac = await _context.Sayaclars.FirstOrDefaultAsync(s => s.SeriNo == dto.YeniSeriNo);
            if (yeniSayac == null)
            {
                return BadRequest(new { message = $"Yeni sayaç {dto.YeniSeriNo} numaralı sistemde bulunamadı!" });
            }

            yeniSayac.TuketimNoktasiId = isEmri.TuketimNoktasiId;
            yeniSayac.Durum = "TAKILI";
            yeniSayac.MuhurNo = dto.YeniMuhurNo;
            yeniSayac.UpdatedAt = DateTime.UtcNow;

            isEmri.SayacId = yeniSayac.SayacId;

            var yeniOkuma = new EndeksOkuma
            {
                SayacId = yeniSayac.SayacId,
                IsEmriId = isEmri.IsEmriId,
                OkumaTipi = "ILK_OKUMA",
                OkumaKaynagi = "MANUEL",
                YeniEndeks = dto.YeniIlkEndeks,
                OkumaZamani = DateTime.UtcNow,
                KullaniciId = dto.IslemYapanKullaniciId,
                DogrulamaDurumu = "ONAYLANDI",
                Status = "AKTIF",
                CreatedAt = DateTime.UtcNow
            };
            _context.EndeksOkumas.Add(yeniOkuma);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new 
            { 
                message = "Sayaç değişimi başarıyla tamamlandı.", 
                isEmriNo = isEmri.IsEmriNo 
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new 
            { 
                message = "Sayaç değişimi sırasında bir hata oluştu.", 
                error = ex.InnerException?.Message ?? ex.Message 
            });
        }
    }

    // 2. Enerji Kesme Operasyonu
    [HttpPost("EnerjiKesme")]
    public async Task<IActionResult> EnerjiKesme([FromBody] EnerjiKesmeRequestDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            var isEmri = await _context.IsEmirleris
                .Include(x => x.Sayac)
                .FirstOrDefaultAsync(x => x.IsEmriId == dto.JobId);

            if (isEmri == null) return NotFound(new { message = "İş emri bulunamadı." });
            if (isEmri.Durum == "TAMAMLANDI") return BadRequest(new { message = "Bu iş emri zaten kapatılmış." });

            isEmri.Durum = "TAMAMLANDI";
            isEmri.KesmeNoktasi = dto.KesmeNoktasi;
            isEmri.KesmeNedeni = dto.KesmeNedeni;
            isEmri.AboneDurumu = dto.AboneDurumu;
            isEmri.SayacDurumu = dto.SayacDurumu;
            isEmri.Gerekce = dto.Aciklama;
            isEmri.MuhurNo = dto.MuhurNo;
            isEmri.SahaSonucu = "Enerji kesme işlemi tamamlandı.";
            isEmri.AtananKullaniciId = dto.IslemYapanKullaniciId;
            isEmri.UpdatedAt = DateTime.UtcNow;

            if (isEmri.Sayac != null)
            {
                isEmri.Sayac.MuhurNo = dto.MuhurNo;
                isEmri.Sayac.UpdatedAt = DateTime.UtcNow;

                var kesmeOkumasi = new EndeksOkuma
                {
                    SayacId = isEmri.Sayac.SayacId,
                    IsEmriId = isEmri.IsEmriId,
                    OkumaTipi = "KESME_ENDEKSI",
                    OkumaKaynagi = "MANUEL",
                    YeniEndeks = dto.SonEndeks,
                    OkumaZamani = DateTime.UtcNow,
                    KullaniciId = dto.IslemYapanKullaniciId,
                    DogrulamaDurumu = "ONAYLANDI",
                    Status = "AKTIF",
                    CreatedAt = DateTime.UtcNow
                };
                _context.EndeksOkumas.Add(kesmeOkumasi);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { message = "Enerji kesme işlemi tamamlandı.", isEmriNo = isEmri.IsEmriNo });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "Enerji kesme işlemi sırasında bir hata oluştu!", error = ex.InnerException?.Message ?? ex.Message });
        }
    }

    // 3. Sayaç Arıza Operasyonu
    [HttpPost("SayacAriza")]
    public async Task<IActionResult> SayacAriza([FromBody] SayacArizaRequestDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            // 1. İş emrini Sayac'ı ile birlikte çek (Eğer sayaç null gelirse SayacId'den manuel bulacağız)
            var isEmri = await _context.IsEmirleris
                .Include(x => x.Sayac)
                .FirstOrDefaultAsync(x => x.IsEmriId == dto.JobId);

            if (isEmri == null) return NotFound(new { message = "İş emri bulunamadı." });
            if (isEmri.Durum == "TAMAMLANDI") return BadRequest(new { message = "Bu iş emri zaten kapatılmış." });

            // 2. Eğer isEmri.Sayac hala null ise, SayacId'den manuel çek (Kritik Racon!)
            if (isEmri.Sayac == null && isEmri.SayacId.HasValue)
            {
                isEmri.Sayac = await _context.Sayaclars.FindAsync(isEmri.SayacId.Value);
            }

            // 3. İş emri statülerini mühürle
            isEmri.Durum = "TAMAMLANDI";
            isEmri.ArizaTipi = dto.ArizaTipi; 
            isEmri.SahaSonucu = dto.SahaSonucu;
            isEmri.TutanakNo = dto.TutanakNo;
            isEmri.AtananKullaniciId = dto.IslemYapanKullaniciId;
            isEmri.UpdatedAt = DateTime.UtcNow;

            // 4. Sayaç işlemleri (Bağlantı sağlandıysa)
            if (isEmri.Sayac != null)
            {
                isEmri.Sayac.Durum = "ARIZALI"; 
                isEmri.Sayac.UpdatedAt = DateTime.UtcNow;

                var arizaOkumasi = new EndeksOkuma
                {
                    SayacId = isEmri.Sayac.SayacId, // Artık null dönmez!
                    IsEmriId = isEmri.IsEmriId,
                    OkumaTipi = "SAYAC_ARIZA_OKUMASI",
                    OkumaKaynagi = "MANUEL", 
                    YeniEndeks = dto.Aktif,
                    GunduzEndeks = dto.Gunduz,
                    PuantEndeks = dto.Puant,
                    GeceEndeks = dto.Gece,
                    InduktifEndeks = dto.Induktif,
                    KapasitifEndeks = dto.Kapasitif,
                    Demand = dto.Demand,
                    OkumaZamani = DateTime.UtcNow,
                    KullaniciId = dto.IslemYapanKullaniciId,
                    DogrulamaDurumu = "ONAYLANDI",
                    Status = "AKTIF",
                    CreatedAt = DateTime.UtcNow
                };
                _context.EndeksOkumas.Add(arizaOkumasi);
            }
            else 
            {
                throw new Exception("İş emrine bağlı hiçbir sayaç bilgisi bulunamadı!");
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { message = "Arıza kaydı başarıyla tamamlandı.", isEmriNo = isEmri.IsEmriNo });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "Arıza kaydı sırasında hata oluştu.", error = ex.Message });
        }
    }

    // 4. Mühürleme Operasyonu
    [HttpPost("SayacMuhurleme")]
    public async Task<IActionResult> Muhurleme([FromBody] SayacMuhurlemeRequestDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            var isEmri = await _context.IsEmirleris
                .Include(x => x.Sayac)
                .FirstOrDefaultAsync(x => x.IsEmriId == dto.JobId);

            if (isEmri == null) return NotFound(new { message = "İş emri bulunamadı." });
            if (isEmri.Durum == "TAMAMLANDI") return BadRequest(new { message = "Bu iş emri zaten tamamlanmış." });

            isEmri.Durum = "TAMAMLANDI";
            isEmri.Gerekce = dto.Gerekce;
            isEmri.EskiMuhurNo = dto.EskiMuhurNo;
            isEmri.YeniMuhurNo = dto.YeniMuhurNo;
            isEmri.TutanakNo = dto.TutanakNo;
            isEmri.SahaSonucu = "Mühürleme işlemi sahada tamamlandı.";
            isEmri.AtananKullaniciId = dto.IslemYapanKullaniciId;
            isEmri.UpdatedAt = DateTime.UtcNow;

            if (isEmri.Sayac != null)
            {
                isEmri.Sayac.MuhurNo = dto.YeniMuhurNo;
                isEmri.Sayac.UpdatedAt = DateTime.UtcNow;

                // 3. Mühürleme Anındaki Tüm Endeksleri Logla (Gigachad Endeks Modeli)
                var muhurOkumasi = new EndeksOkuma
                {
                    SayacId = isEmri.Sayac.SayacId,
                    IsEmriId = isEmri.IsEmriId,
                    OkumaTipi = "MUHURLEME_ENDEKSI",
                    OkumaKaynagi = "MANUEL",
                    YeniEndeks = dto.Aktif,
                    GunduzEndeks = dto.Gunduz,
                    PuantEndeks = dto.Puant,
                    GeceEndeks = dto.Gece,
                    InduktifEndeks = dto.Induktif,
                    KapasitifEndeks = dto.Kapasitif,
                    Demand = dto.Demand,
                    OkumaZamani = DateTime.UtcNow,
                    KullaniciId = dto.IslemYapanKullaniciId,
                    DogrulamaDurumu = "ONAYLANDI",
                    Status = "AKTIF",
                    CreatedAt = DateTime.UtcNow
                };
                _context.EndeksOkumas.Add(muhurOkumasi);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { message = "Mühürleme işlemi başarıyla tamamlandı.", isEmriNo = isEmri.IsEmriNo });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "Mühürleme işlemi sırasında bir hata oluştu.", error = ex.InnerException?.Message ?? ex.Message });
        }
    }

    // 5. Yeni Bağlantı Operasyonu
    [HttpPost("SayacYeniBaglanti")]
    public async Task<IActionResult> YeniBaglanti([FromBody] SayacYeniBaglantiRequestDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            var isEmri = await _context.IsEmirleris
                .FirstOrDefaultAsync(x => x.IsEmriId == dto.JobId);

            if (isEmri == null) return NotFound(new { message = "Böyle bir iş emri bulunamadı." });
            if (isEmri.Durum == "TAMAMLANDI") return BadRequest(new { message = "Bu iş emri zaten tamamlanmış." });

            // 1. İş Emrini Kapat
            isEmri.Durum = "TAMAMLANDI";
            isEmri.YeniSeriNo = dto.SeriNo; // Takılan sayacın seri nosu
            isEmri.YapiTesisTipi = dto.BaglantiTipi;
            isEmri.DamgaYili = dto.DamgaYili;
            isEmri.YeniMuhurNo = dto.YeniMuhurNo;
            isEmri.SahaSonucu = "Yeni bağlantı başarıyla sağlandı.";
            isEmri.AtananKullaniciId = dto.IslemYapanKullaniciId;
            isEmri.UpdatedAt = DateTime.UtcNow;

            // 2. Depodaki Sayacı Bul ve Mekana Zımbala
            var sayac = await _context.Sayaclars.FirstOrDefaultAsync(s => s.SeriNo == dto.SeriNo);
            if (sayac == null) 
                return BadRequest(new { message = $" {dto.SeriNo} numaralı sayaç sistemde (depoda) bulunamadı!" });

            // Veritabanı raconuna tam uyum: Durumu "TAKILI" yapıyoruz
            sayac.Durum = "TAKILI";
            sayac.TuketimNoktasiId = isEmri.TuketimNoktasiId; // Sayacı mekana (Tüketim Noktasına) bağlıyoruz
            sayac.MuhurNo = dto.YeniMuhurNo;
            sayac.UpdatedAt = DateTime.UtcNow;

            // İş emrinin referansını yeni bağlanan sayaca kaydırıyoruz
            isEmri.SayacId = sayac.SayacId;

            // 3. Sayacın İlk Endeksini (Genelde 0 veya düşük bir değer) Çak
            var ilkOkuma = new EndeksOkuma
            {
                SayacId = sayac.SayacId,
                IsEmriId = isEmri.IsEmriId,
                OkumaTipi = "ILK_OKUMA",
                OkumaKaynagi = "MANUEL",
                YeniEndeks = dto.IlkEndeks, // Sadece aktif endeks geliyor
                OkumaZamani = DateTime.UtcNow,
                KullaniciId = dto.IslemYapanKullaniciId,
                DogrulamaDurumu = "ONAYLANDI",
                Status = "AKTIF",
                CreatedAt = DateTime.UtcNow
            };
            _context.EndeksOkumas.Add(ilkOkuma);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { message = "Yeni bağlantı başarıyla sağlandı.", isEmriNo = isEmri.IsEmriNo });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "Yeni bağlantı oluşturulurken bir hata oluştu.", error = ex.InnerException?.Message ?? ex.Message });
        }
    }

    // 6. Keşif Raporlama Operasyonu
    [HttpPost("KesifRaporlama")]
    public async Task<IActionResult> KesifRaporlama([FromBody] KesifRaporlamaRequestDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            var isEmri = await _context.IsEmirleris
                .FirstOrDefaultAsync(x => x.IsEmriId == dto.JobId);

            if (isEmri == null) return NotFound(new { message = "Böyle bir iş emri bulunamadı." });
            if (isEmri.Durum == "TAMAMLANDI") return BadRequest(new { message = "Bu keşif zaten raporlanmış." });

            // 1. İş Emrini Keşif Detaylarıyla Zırhlandır
            isEmri.Durum = "TAMAMLANDI";
            isEmri.PanoDirekNo = dto.PanoDirekNo;
            isEmri.KesifSonucu = dto.KesifSonucu;
            isEmri.YapiTesisTipi = dto.YapiTesisTipi;
            isEmri.HatMesafesi = dto.HatMesafesi;
            isEmri.TalepGucu = dto.TalepGucu;
            isEmri.IncelemeNotu = dto.IncelemeNotu;
            isEmri.SahaSonucu = "Keşif ve şebeke incelemesi tamamlandı.";
            isEmri.AtananKullaniciId = dto.IslemYapanKullaniciId;
            isEmri.UpdatedAt = DateTime.UtcNow;

            // Burada sayaç veya endeks tablosuyla işimiz yok, direkt fişi çekiyoruz
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { message = "Keşif raporu başarıyla oluşturuldu.", isEmriNo = isEmri.IsEmriNo });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "Keşif raporu oluşturulurken bir hata oluştu.", error = ex.InnerException?.Message ?? ex.Message });
        }
    }
}

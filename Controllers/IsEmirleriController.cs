using KcetasAboneApi.DTOs.IsEmirleri;
using KcetasAboneApi.Hubs;
using KcetasAboneApi.Models;
using KcetasAboneApi.Models.Dtos;
using KcetasAboneApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace KcetasAboneApi.Controllers;

//[Authorize(Roles = "1, 5")]
[Route("api/[controller]")]
[ApiController]
public class IsEmirleriController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IIsEmiriService _isEmiriService;
    private readonly IHubContext<BildirimHub> _hubContext;

    public IsEmirleriController(AppDbContext context, IIsEmiriService isEmiriService, IHubContext<BildirimHub> hubContext)
    {
        _context = context;
        _isEmiriService = isEmiriService;
        _hubContext = hubContext;
    }

    // GET: api/IsEmirleri/All
    [HttpGet("All")]
    public async Task<ActionResult<IEnumerable<IsEmriListDto>>> GetAllIsEmirleri([FromQuery] bool includeCompleted = false)
    {

        var query = _context.IsEmirleris.AsQueryable();

        if (!includeCompleted)
        {
            query = query.Where(i => i.Durum != IsEmriDurumu.TAMAMLANDI);
        }

        return await query
    .Include(i => i.Sayac)
    .Include(i => i.TuketimNoktasi)
    .OrderBy(i => i.IsEmriId)
    .Select(i => new IsEmriListDto
    {
        IsEmriId = i.IsEmriId,
        IsEmriNo = i.IsEmriNo,
        Tip = i.Tip,
        Durum = i.Durum,
        TuketimNoktasiId = i.TuketimNoktasiId,
        SayacId = i.SayacId,
        AtananKullaniciId = i.AtananKullaniciId,
        PlanlananTarih = i.PlanlananTarih,
        CreatedAt = i.CreatedAt,

        SayacSeriNo = i.Sayac != null ? i.Sayac.SeriNo : null,
        Adres = i.TuketimNoktasi != null ? i.TuketimNoktasi.AcikAdres : null,

        SozlesmeNo = null,
        AboneAdi = null,
        AboneNo = null
    })
    .ToListAsync();
    }
    // GET: api/IsEmirleri/Kullanici/5
    [HttpGet("Kullanici/{kullaniciId}")]
    public async Task<ActionResult<IEnumerable<IsEmriListDto>>> GetKullaniciIsEmirleri(
        long kullaniciId,
        [FromQuery] bool includeCompleted = false)
    {
        var query = _context.IsEmirleris
            .AsNoTracking()
            .Where(i => i.AtananKullaniciId == kullaniciId && i.Durum != IsEmriDurumu.IPTAL);

        if (!includeCompleted)
        {
            query = query.Where(i => i.Durum != IsEmriDurumu.TAMAMLANDI);
        }

        var isEmirleri = await query
        .OrderBy(i => i.IsEmriId)
        .Select(i => new IsEmriListDto
        {
            IsEmriId = i.IsEmriId,
            IsEmriNo = i.IsEmriNo,
            Tip = i.Tip,
            Durum = i.Durum,
            TuketimNoktasiId = i.TuketimNoktasiId,
            SayacId = i.SayacId,
            AtananKullaniciId = i.AtananKullaniciId,
            PlanlananTarih = i.PlanlananTarih,
            CreatedAt = i.CreatedAt,

            SayacSeriNo = i.Sayac != null ? i.Sayac.SeriNo : null,
            Adres = i.TuketimNoktasi != null ? i.TuketimNoktasi.AcikAdres : null,

            SozlesmeNo = _context.Sozlesmelers
                .Where(s => s.TuketimNoktasiId == i.TuketimNoktasiId && s.Durum == SozlesmeDurumu.AKTIF)
                .Select(s => s.SozlesmeNo)
                .FirstOrDefault(),

            AboneNo = _context.Sozlesmelers
                .Where(s => s.TuketimNoktasiId == i.TuketimNoktasiId && s.Durum == SozlesmeDurumu.AKTIF)
                .Select(s => s.Abone!.AboneNo)
                .FirstOrDefault(),

            AboneAdi = _context.Sozlesmelers
                .Where(s => s.TuketimNoktasiId == i.TuketimNoktasiId && s.Durum == SozlesmeDurumu.AKTIF)
                .Select(s => string.IsNullOrEmpty(s.Abone!.Ad) ? s.Abone.Unvan : (s.Abone.Ad + " " + s.Abone.Soyad))
                .FirstOrDefault()
        })
        .ToListAsync();

        return Ok(isEmirleri);
    }

    // GET: api/IsEmirleri
    [HttpGet]
    public async Task<IActionResult> GetIsEmirleri([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] bool includeCompleted = false)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var query = _context.IsEmirleris.AsQueryable();

        if (!includeCompleted) 
        {
            query = query.Where(i => i.Durum != IsEmriDurumu.TAMAMLANDI);
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var isEmirleri = await query
    .Include(i => i.Sayac)
    .Include(i => i.TuketimNoktasi)
    .OrderBy(i => i.IsEmriId)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .Select(i => new IsEmriListDto
    {
        IsEmriId = i.IsEmriId,
        IsEmriNo = i.IsEmriNo,
        Tip = i.Tip,
        Durum = i.Durum,
        TuketimNoktasiId = i.TuketimNoktasiId,
        SayacId = i.SayacId,
        AtananKullaniciId = i.AtananKullaniciId,
        PlanlananTarih = i.PlanlananTarih,
        CreatedAt = i.CreatedAt,

        SayacSeriNo = i.Sayac != null ? i.Sayac.SeriNo : null,
        Adres = i.TuketimNoktasi != null ? i.TuketimNoktasi.AcikAdres : null,

        // Bunları sonraki adımda dolduracağız
        SozlesmeNo = null,
        AboneAdi = null,
        AboneNo = null
    })
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
        .Where(x => x.IsEmriNo == temizlenmisNo)
        .Select(i => new IsEmriListDto
        {
            IsEmriId = i.IsEmriId,
            IsEmriNo = i.IsEmriNo,
            Tip = i.Tip,
            Durum = i.Durum,
            TuketimNoktasiId = i.TuketimNoktasiId,
            SayacId = i.SayacId,
            AtananKullaniciId = i.AtananKullaniciId,
            PlanlananTarih = i.PlanlananTarih,
            CreatedAt = i.CreatedAt,

            SayacSeriNo = i.Sayac != null ? i.Sayac.SeriNo : null,
            Adres = i.TuketimNoktasi != null ? i.TuketimNoktasi.AcikAdres : null,

            SozlesmeNo = null,
            AboneAdi = null,
            AboneNo = null
        })
        .FirstOrDefaultAsync();

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
    public async Task<ActionResult<IsEmriListDto>> GetIsEmri(long id)
    {
        var isEmri = await _context.IsEmirleris
    .Include(i => i.Sayac)
    .Include(i => i.TuketimNoktasi)
    .Where(i => i.IsEmriId == id)
    .Select(i => new IsEmriListDto
    {
        IsEmriId = i.IsEmriId,
        IsEmriNo = i.IsEmriNo,
        Tip = i.Tip,
        Durum = i.Durum,
        TuketimNoktasiId = i.TuketimNoktasiId,
        SayacId = i.SayacId,
        AtananKullaniciId = i.AtananKullaniciId,
        PlanlananTarih = i.PlanlananTarih,
        CreatedAt = i.CreatedAt,

        SayacSeriNo = i.Sayac != null ? i.Sayac.SeriNo : null,
        Adres = i.TuketimNoktasi != null ? i.TuketimNoktasi.AcikAdres : null,

        SozlesmeNo = null,
        AboneAdi = null,
        AboneNo = null
    })
    .FirstOrDefaultAsync();

        if (isEmri == null)
        {
            return NotFound(new { message = "İş emri bulunamadı." });
        }

        return Ok(isEmri);
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
        else if (dto.Tip != IsEmriTipi.YENI_BAGLANTI)
        {
            var takiliSayac = await _context.Sayaclars
                .FirstOrDefaultAsync(s => s.TuketimNoktasiId == dto.TuketimNoktasiId && (s.Durum == SayacDurumu.TAKILI));
            
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

    // SILME: api/IsEmirleri/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> SILMEIsEmri(long id)
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
        var result = await _isEmiriService.IsEmriTamamlaAsync(isEmriId, sayacId);
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }

    [HttpPost("toplu-yeni-baglanti-onayla")]
    public async Task<IActionResult> TopluYeniBaglantiOnayla()
    {
        var result = await _isEmiriService.TopluYeniBaglantiOnaylaAsync();
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message, onaylananSayi = result.ProcessedCount });
    }

    [HttpPost("generate-random-is-emirleri")]
    public async Task<IActionResult> GenerateRandomIsEmirleri()
    {

        var takiliSayaclar = await _context.Sayaclars
            .Where(s => s.Durum == SayacDurumu.TAKILI && s.TuketimNoktasiId != null)
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
            IsEmriTipi.DEGISTIRME, IsEmriTipi.SOKME, IsEmriTipi.KESME, IsEmriTipi.ACMA, 
            IsEmriTipi.ENDEKS_OKUMA, IsEmriTipi.SAYAC_ARIZA, IsEmriTipi.MUHURLEME, IsEmriTipi.KESIF_INCELEME, IsEmriTipi.ENERJI_ACMA 
        };
        
        var oncelikler = new[] { "DUSUK", "NORMAL", "YUKSEK", "ACIL" };
        var sahteIsEmirleri = new List<IsEmirleri>();
        var random = new Random();

        for (int j = 0; j < 30; j++)
        {
            var secilenSayac = takiliSayaclar[random.Next(takiliSayaclar.Count)];
            IsEmriTipi secilenTip = isEmriTipleri[random.Next(isEmriTipleri.Length)];

            var yeniIsEmri = new IsEmirleri
            {
                IsEmriNo = $"{isEmriPrefix}{(isEmriSira + j):D4}",
                TuketimNoktasiId = secilenSayac.TuketimNoktasiId ?? 0,
                SayacId = secilenSayac.SayacId, 
                Tip = secilenTip,

                Oncelik = (secilenTip == IsEmriTipi.SAYAC_ARIZA || secilenTip == IsEmriTipi.KESME) ? "ACIL" : oncelikler[random.Next(oncelikler.Length)],
                
                Durum = IsEmriDurumu.ACIK,
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
                
            if (isEmri.Durum == IsEmriDurumu.TAMAMLANDI) 
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
            isEmri.Durum = IsEmriDurumu.TAMAMLANDI;
            if (!string.IsNullOrEmpty(request.SahaSonucu))
                isEmri.SahaSonucu = request.SahaSonucu;
            
            isEmri.UpdatedAt = DateTime.UtcNow;

            var sonOkuma = await _context.EndeksOkumas
                .Where(e => e.SayacId == sayacId)
                .OrderByDescending(e => e.CreatedAt)
                .FirstOrDefaultAsync();

            OkumaTipi dinamikOkumaTipi = isEmri.Tip switch
            {
                IsEmriTipi.KESME => OkumaTipi.KESME_ENDEKSI,
                IsEmriTipi.DEGISTIRME => OkumaTipi.SAYAC_DEGISIM_OKUMASI,
                IsEmriTipi.SAYAC_ARIZA => OkumaTipi.SAYAC_ARIZA_OKUMASI, 
                IsEmriTipi.MUHURLEME => OkumaTipi.MUHURLEME_ENDEKSI, 
                IsEmriTipi.SOKME => OkumaTipi.SON_OKUMA,
                IsEmriTipi.ENERJI_ACMA => OkumaTipi.ILK_OKUMA, 
                IsEmriTipi.ACMA => OkumaTipi.ILK_OKUMA,
                IsEmriTipi.YENI_BAGLANTI => OkumaTipi.ILK_OKUMA, 
                _ => sonOkuma == null ? OkumaTipi.ILK_OKUMA : OkumaTipi.RUTIN_DONEM
            };

            var yeniEndeks = new EndeksOkuma
            {
                SayacId = sayacId,
                IsEmriId = isEmri.IsEmriId,
                OkumaTipi = dinamikOkumaTipi,
                OkumaKaynagi = OkumaKaynagi.MANUEL,
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
                DogrulamaDurumu = DogrulamaDurumu.ONAYLANDI,
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
                    IsEmriTipi.SOKME => SayacDurumu.SOKULMUS,
                    IsEmriTipi.DEGISTIRME => SayacDurumu.SOKULMUS,
                    IsEmriTipi.SAYAC_ARIZA => SayacDurumu.ARIZALI,
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

        if (isEmri.Durum == IsEmriDurumu.TAMAMLANDI)
            return BadRequest(new { message = "Bu iş emri zaten tamamlanmış." });

        isEmri.Durum = IsEmriDurumu.TAMAMLANDI;
        isEmri.SahaSonucu = "Enerji Açıldı - Nokta: " + dto.AcmaNoktasi;
        isEmri.MuhurNo = dto.MuhurNo;
        isEmri.Gerekce = dto.Aciklama;
        isEmri.UpdatedAt = DateTime.UtcNow;

        if (isEmri.SayacId.HasValue && isEmri.TuketimNoktasiId > 0)
        {
            var sozlesme = await _context.Sozlesmelers
                .FirstOrDefaultAsync(s => s.TuketimNoktasiId == isEmri.TuketimNoktasiId && s.Durum == SozlesmeDurumu.AKTIF);

            var okuma = new EndeksOkuma
            {
                SayacId = isEmri.SayacId.Value,
                IsEmriId = isEmri.IsEmriId,
                SozlesmeId = sozlesme?.SozlesmeId,
                OkumaTipi = OkumaTipi.ILK_OKUMA,
                OkumaKaynagi = OkumaKaynagi.MANUEL,
                OncekiEndeks = 0,
                YeniEndeks = dto.Aktif,
                OkumaZamani = DateTime.UtcNow,
                DogrulamaDurumu = DogrulamaDurumu.ONAYLANDI,
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
            var isEmri = await _context.IsEmirleris
                .FirstOrDefaultAsync(x => x.IsEmriId == dto.JobId);

            if (isEmri == null) 
                return NotFound(new { message = "Böyle bir iş emri bulunamadı." });
                
            if (isEmri.Durum == IsEmriDurumu.TAMAMLANDI) 
                return BadRequest(new { message = "Bu iş emri zaten tamamlanmış!" });

            isEmri.Durum = IsEmriDurumu.TAMAMLANDI;
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
                eskiSayac.Durum = SayacDurumu.SOKULMUS;
                eskiSayac.TuketimNoktasiId = null;
                eskiSayac.UpdatedAt = DateTime.UtcNow;

                var eskiOkuma = new EndeksOkuma
                {
                    SayacId = eskiSayac.SayacId,
                    IsEmriId = isEmri.IsEmriId,
                    OkumaTipi = OkumaTipi.SAYAC_DEGISIM_OKUMASI,
                    OkumaKaynagi = OkumaKaynagi.MANUEL,
                    YeniEndeks = dto.SokulenAktif, 
                    OkumaZamani = DateTime.UtcNow,
                    KullaniciId = dto.IslemYapanKullaniciId,
                    DogrulamaDurumu = DogrulamaDurumu.ONAYLANDI,
                    Status = "AKTIF",
                    CreatedAt = DateTime.UtcNow
                };
                _context.EndeksOkumas.Add(eskiOkuma);
            }

            var yeniSayac = await _context.Sayaclars.FirstOrDefaultAsync(s => s.SeriNo == dto.YeniSeriNo);
            if (yeniSayac == null)
            {
                return BadRequest(new { message = $"Yeni sayaç {dto.YeniSeriNo} numaralı sistemde bulunamadı!" });
            }

            yeniSayac.TuketimNoktasiId = isEmri.TuketimNoktasiId;
            yeniSayac.Durum = SayacDurumu.TAKILI;
            yeniSayac.MuhurNo = dto.YeniMuhurNo;
            yeniSayac.UpdatedAt = DateTime.UtcNow;

            isEmri.SayacId = yeniSayac.SayacId;

            var yeniOkuma = new EndeksOkuma
            {
                SayacId = yeniSayac.SayacId,
                IsEmriId = isEmri.IsEmriId,
                OkumaTipi = OkumaTipi.ILK_OKUMA,
                OkumaKaynagi = OkumaKaynagi.MANUEL,
                YeniEndeks = dto.YeniIlkEndeks,
                OkumaZamani = DateTime.UtcNow,
                KullaniciId = dto.IslemYapanKullaniciId,
                DogrulamaDurumu = DogrulamaDurumu.ONAYLANDI,
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
            if (isEmri.Durum == IsEmriDurumu.TAMAMLANDI) return BadRequest(new { message = "Bu iş emri zaten kapatılmış." });

            isEmri.Durum = IsEmriDurumu.TAMAMLANDI;
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
                    OkumaTipi = OkumaTipi.KESME_ENDEKSI,
                    OkumaKaynagi = OkumaKaynagi.MANUEL,
                    YeniEndeks = dto.SonEndeks,
                    OkumaZamani = DateTime.UtcNow,
                    KullaniciId = dto.IslemYapanKullaniciId,
                    DogrulamaDurumu = DogrulamaDurumu.ONAYLANDI,
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

            var isEmri = await _context.IsEmirleris
                .Include(x => x.Sayac)
                .FirstOrDefaultAsync(x => x.IsEmriId == dto.JobId);

            if (isEmri == null) return NotFound(new { message = "İş emri bulunamadı." });
            if (isEmri.Durum == IsEmriDurumu.TAMAMLANDI) return BadRequest(new { message = "Bu iş emri zaten kapatılmış." });

            if (isEmri.Sayac == null && isEmri.SayacId.HasValue)
            {
                isEmri.Sayac = await _context.Sayaclars.FindAsync(isEmri.SayacId.Value);
            }

            isEmri.Durum = IsEmriDurumu.TAMAMLANDI;
            isEmri.ArizaTipi = dto.ArizaTipi; 
            isEmri.SahaSonucu = dto.SahaSonucu;
            isEmri.TutanakNo = dto.TutanakNo;
            isEmri.AtananKullaniciId = dto.IslemYapanKullaniciId;
            isEmri.UpdatedAt = DateTime.UtcNow;

            if (isEmri.Sayac != null)
            {
                isEmri.Sayac.Durum = SayacDurumu.ARIZALI; 
                isEmri.Sayac.UpdatedAt = DateTime.UtcNow;

                var arizaOkumasi = new EndeksOkuma
                {
                    SayacId = isEmri.Sayac.SayacId, 
                    IsEmriId = isEmri.IsEmriId,
                    OkumaTipi = OkumaTipi.SAYAC_ARIZA_OKUMASI,
                    OkumaKaynagi = OkumaKaynagi.MANUEL, 
                    YeniEndeks = dto.Aktif,
                    GunduzEndeks = dto.Gunduz,
                    PuantEndeks = dto.Puant,
                    GeceEndeks = dto.Gece,
                    InduktifEndeks = dto.Induktif,
                    KapasitifEndeks = dto.Kapasitif,
                    Demand = dto.Demand,
                    OkumaZamani = DateTime.UtcNow,
                    KullaniciId = dto.IslemYapanKullaniciId,
                    DogrulamaDurumu = DogrulamaDurumu.ONAYLANDI,
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
            if (isEmri.Durum == IsEmriDurumu.TAMAMLANDI) return BadRequest(new { message = "Bu iş emri zaten tamamlanmış." });

            isEmri.Durum = IsEmriDurumu.TAMAMLANDI;
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

                var muhurOkumasi = new EndeksOkuma
                {
                    SayacId = isEmri.Sayac.SayacId,
                    IsEmriId = isEmri.IsEmriId,
                    OkumaTipi = OkumaTipi.MUHURLEME_ENDEKSI,
                    OkumaKaynagi = OkumaKaynagi.MANUEL,
                    YeniEndeks = dto.Aktif,
                    GunduzEndeks = dto.Gunduz,
                    PuantEndeks = dto.Puant,
                    GeceEndeks = dto.Gece,
                    InduktifEndeks = dto.Induktif,
                    KapasitifEndeks = dto.Kapasitif,
                    Demand = dto.Demand,
                    OkumaZamani = DateTime.UtcNow,
                    KullaniciId = dto.IslemYapanKullaniciId,
                    DogrulamaDurumu = DogrulamaDurumu.ONAYLANDI,
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
            if (isEmri.Durum == IsEmriDurumu.TAMAMLANDI) return BadRequest(new { message = "Bu iş emri zaten tamamlanmış." });

            isEmri.Durum = IsEmriDurumu.TAMAMLANDI;
            isEmri.YeniSeriNo = dto.SeriNo; 
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

            sayac.Durum = SayacDurumu.TAKILI;
            sayac.TuketimNoktasiId = isEmri.TuketimNoktasiId; 
            sayac.MuhurNo = dto.YeniMuhurNo;
            sayac.UpdatedAt = DateTime.UtcNow;

            isEmri.SayacId = sayac.SayacId;

            var ilkOkuma = new EndeksOkuma
            {
                SayacId = sayac.SayacId,
                IsEmriId = isEmri.IsEmriId,
                OkumaTipi = OkumaTipi.ILK_OKUMA,
                OkumaKaynagi = OkumaKaynagi.MANUEL,
                YeniEndeks = dto.IlkEndeks, 
                OkumaZamani = DateTime.UtcNow,
                KullaniciId = dto.IslemYapanKullaniciId,
                DogrulamaDurumu = DogrulamaDurumu.ONAYLANDI,
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
            if (isEmri.Durum == IsEmriDurumu.TAMAMLANDI) return BadRequest(new { message = "Bu keşif zaten raporlanmış." });

            isEmri.Durum = IsEmriDurumu.TAMAMLANDI;
            isEmri.PanoDirekNo = dto.PanoDirekNo;
            isEmri.KesifSonucu = dto.KesifSonucu;
            isEmri.YapiTesisTipi = dto.YapiTesisTipi;
            isEmri.HatMesafesi = dto.HatMesafesi;
            isEmri.TalepGucu = dto.TalepGucu;
            isEmri.IncelemeNotu = dto.IncelemeNotu;
            isEmri.SahaSonucu = "Keşif ve şebeke incelemesi tamamlandı.";
            isEmri.AtananKullaniciId = dto.IslemYapanKullaniciId;
            isEmri.UpdatedAt = DateTime.UtcNow;

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

    [HttpPost("atama-yap")]
    public async Task<IActionResult> UstayaIsAta([FromBody] IsEmriAtamaRequestDto request)
    {

        var isEmri = await _context.IsEmirleris.FindAsync(request.IsEmriId);
        if (isEmri == null) 
            return NotFound(new { message = "Böyle bir iş emri bulunamadı." });

        if (isEmri.Durum == IsEmriDurumu.TAMAMLANDI) 
            return BadRequest(new { message = "Tamamlanmış işe personel atanamaz." });

        isEmri.AtananKullaniciId = request.UstaId;
        isEmri.Durum = IsEmriDurumu.ATANDI;
        isEmri.UpdatedAt = DateTime.UtcNow;

        var mesaj = $"{isEmri.IsEmriNo} numaralı iş emri atandı.";
        var yeniBildirim = new Bildirim
        {
            KullaniciId = (int)request.UstaId, 
            Baslik = "Yeni Görev",
            Icerik = mesaj,
            IsEmriId = isEmri.IsEmriId,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Bildirimler.Add(yeniBildirim);
        await _context.SaveChangesAsync();

        await _hubContext.Clients.User(request.UstaId.ToString())
            .SendAsync("YeniBildirimGeldi", "Yeni Görev", mesaj, isEmri.IsEmriId); 

        return Ok(new { message = "İş atandı, bildirim gönderildi.", isEmriNo = isEmri.IsEmriNo });
    }

    [HttpPost("otomatik-atama")]
    public async Task<IActionResult> OtomatikAtamaYap()
    {
        var sahipsizIsEmirleri = await _context.IsEmirleris
            .Where(i => i.AtananKullaniciId == null && i.Durum == IsEmriDurumu.ACIK)
            .ToListAsync();

        var sahaEkipleri = await _context.Kullanicilars
            .Where(k => k.RolId == 1)
            .ToListAsync();

        if (!sahipsizIsEmirleri.Any() || !sahaEkipleri.Any())
            return BadRequest(new { message = "Atanacak iş veya personel kalmadı." });

        int atananSayisi = 0;
        int ekipIndex = 0;

        var atananBildirimler = new List<Bildirim>();

        foreach (var isEmri in sahipsizIsEmirleri)
        {
            var secilenUstaId = sahaEkipleri[ekipIndex].KullaniciId;
            
            // İşi ustaya zimmetle
            isEmri.AtananKullaniciId = secilenUstaId;
            isEmri.Durum = IsEmriDurumu.ATANDI;
            isEmri.UpdatedAt = DateTime.UtcNow;
            
            // Bildirim paketini hazırla
            var mesaj = $"{isEmri.IsEmriNo} numaralı görev otomatik atama ile atandı.";
            atananBildirimler.Add(new Bildirim
            {
                KullaniciId = (int)secilenUstaId,
                Baslik = "Otomatik Görev",
                Icerik = mesaj,
                IsEmriId = isEmri.IsEmriId, 
                CreatedAt = DateTime.UtcNow
            });

            // Sıradaki ekibe geç
            ekipIndex++;
            if (ekipIndex >= sahaEkipleri.Count) ekipIndex = 0; 
            
            atananSayisi++;
        }

        // 1. Önce tüm bildirimleri DB'ye tek seferde kaydet
        _context.Bildirimler.AddRange(atananBildirimler);
        await _context.SaveChangesAsync();

        // 2. Döngüyle herkese ayrı ayrı SignalR pop-up füzesi at
        foreach (var bildirim in atananBildirimler)
        {

            await _hubContext.Clients.User(bildirim.KullaniciId.ToString())
                .SendAsync("YeniBildirimGeldi", bildirim.Baslik, bildirim.Icerik, bildirim.IsEmriId);
        }

        return Ok(new { message = $"{atananSayisi} iş emri ekiplere dağıtıldı ve bildirim gönderildi." });
    }

    [HttpPost("planlanan-tarihleri-dagit")]
    public async Task<IActionResult> PlanlananTarihleriDagit()
    {

        var isEmirleri = await _context.IsEmirleris
            .Where(i => i.Durum == IsEmriDurumu.ATANDI && i.PlanlananTarih == null)
            .ToListAsync();

        if (!isEmirleri.Any())
            return BadRequest(new { message = "Planlanan tarihi boş olan iş emri kalmadı." });

        var random = new Random();
        var bugun = DateTime.UtcNow; 

        foreach (var isEmri in isEmirleri)
        {
            // 1 ile 7 gün sonrasına rastgele atıyoruz
            int rastgeleGun = random.Next(1, 8); 


            // Tarihi oluştur ve iş emrine zımbala
            var yeniTarih = bugun.AddDays(rastgeleGun).Date;

            isEmri.PlanlananTarih = DateTime.SpecifyKind(yeniTarih, DateTimeKind.Utc);
            isEmri.UpdatedAt = DateTime.UtcNow;
        }

        // 3. Harcı veritabanına dök!
        await _context.SaveChangesAsync();

        return Ok(new 
        { 
            message = $"{isEmirleri.Count} adet iş emrinin planlanan tarihi önümüzdeki 1 haftaya dağıtıldı!",
            guncellenenKayit = isEmirleri.Count
        });
    }
}

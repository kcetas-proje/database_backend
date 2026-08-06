using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KcetasAboneApi.Models;

namespace KcetasAboneApi.Controllers;

//[Authorize(Roles = "1, 3")]
[Route("api/[controller]")]
[ApiController]
public class SozlesmelerController : ControllerBase
{
    private readonly AppDbContext _context;

    public SozlesmelerController(AppDbContext context) 
    {
        _context = context;
    }

    // GET: api/Sozlesmeler
    /// <summary>
    /// Sistemdeki tüm sözleşmeleri getirir.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Sozlesmeler>>> GetSozlesmeler()
    {
        var result = await _context.Sozlesmelers.ToListAsync();
        return result.OrderBy(s => s.SozlesmeId).ToList();
    }

    // GET: api/Sozlesmeler/5
    /// <summary>
    /// ID'si verilen sözleşmenin detaylarını getirir.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Sozlesmeler>> GetSozlesme(long id)
    {
        var sozlesme = await _context.Sozlesmelers.FindAsync(id);

        if (sozlesme == null)
        {
            return NotFound(new
            {
                message = "Sözleşme bulunamadı."
            });
        }

        return sozlesme;
    }

    /// <summary>
    /// Sözleşmeleri sayfalanmış ve filtrelenebilir olarak getirir.
    /// </summary>
    [HttpGet("Paged")]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? q = null,
        [FromQuery] SozlesmeDurumu? durum = null,
        [FromQuery] string? tekilKod = null,
        [FromQuery] string? sozlesmeTipi = null)
    {
        var query = _context.Sozlesmelers
            .Include(s => s.TuketimNoktasi)
            .AsNoTracking()
            .AsQueryable();

        if (durum.HasValue)
        {
            query = query.Where(s => s.Durum == durum.Value);
        }

        if (!string.IsNullOrWhiteSpace(sozlesmeTipi))
        {
            var stLower = sozlesmeTipi.ToLower();
            query = query.Where(s => s.SozlesmeTipi.ToLower() == stLower);
        }

        if (!string.IsNullOrWhiteSpace(tekilKod))
        {
            var tkLower = tekilKod.ToLower();
            query = query.Where(s => s.TuketimNoktasi != null && s.TuketimNoktasi.TekilKod.ToLower().Contains(tkLower));
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            var qLower = q.ToLower();
            bool isNumeric = long.TryParse(q, out long numQ);

            query = query.Where(s => 
                s.SozlesmeNo.ToLower().Contains(qLower) || 
                (s.TuketimNoktasi != null && s.TuketimNoktasi.TekilKod.ToLower().Contains(qLower)) ||
                (isNumeric && (s.SozlesmeId == numQ || s.TuketimNoktasiId == numQ))
            );
        }

        var total = await query.CountAsync();

        var data = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new
            {
                s.SozlesmeId,
                s.SozlesmeNo,
                s.AboneId,
                s.TuketimNoktasiId,
                s.TarifeId,
                s.SozlesmeTipi,
                s.GuvenceBedeli,
                s.BaslangicTarihi,
                s.BitisTarihi,
                Durum = s.Durum.ToString(),
                s.CreatedAt,
                s.UpdatedAt,
                TekilKod = s.TuketimNoktasi != null ? s.TuketimNoktasi.TekilKod : null
            })
            .ToListAsync();

        return Ok(new
        {
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            Data = data
        });
    }

    // POST: api/Sozlesmeler
    /// <summary>
    /// Yeni bir sözleşme oluşturur ve otomatik olarak 'Açma' iş emri başlatır.
    /// </summary>
    [HttpPost]
public async Task<ActionResult<Sozlesmeler>> PostSozlesme(SozlesmeCreateDto dto)
{
    var abone = await _context.Abonelers.FirstOrDefaultAsync(x => x.AboneId == dto.AboneId);
    if (abone == null)
        return BadRequest(new { message = "Abone bulunamadı." });

    if (abone.Status != "AKTIF")
        return BadRequest(new { message = "Pasif durumdaki aboneye yeni sözleşme oluşturulamaz." });

    if (!await _context.TuketimNoktasis.AnyAsync(x => x.TuketimNoktasiId == dto.TuketimNoktasiId))
        return BadRequest(new { message = "Tüketim noktası bulunamadı." });

    if (!await _context.Tarifelers.AnyAsync(x => x.TarifeId == dto.TarifeId))
        return BadRequest(new { message = "Tarife bulunamadı." });

    string sozlesmePrefix = $"SOZ-{DateTime.UtcNow:yyyy-MM}-";
    var sonSozlesme = await _context.Sozlesmelers
        .Where(x => x.SozlesmeNo.StartsWith(sozlesmePrefix))
        .OrderByDescending(x => x.SozlesmeNo)
        .FirstOrDefaultAsync();

    int sozlesmeSira = 1;
    if (sonSozlesme != null && int.TryParse(sonSozlesme.SozlesmeNo.Substring(sonSozlesme.SozlesmeNo.Length - 4), out int sSira))
        sozlesmeSira = sSira + 1;

    string yeniSozlesmeNo = $"{sozlesmePrefix}{sozlesmeSira:D4}";

    var yeniSozlesme = new Sozlesmeler
    {
        SozlesmeNo = yeniSozlesmeNo,
        AboneId = dto.AboneId,
        TuketimNoktasiId = dto.TuketimNoktasiId,
        TarifeId = dto.TarifeId,
        SozlesmeTipi = dto.SozlesmeTipi,
        GuvenceBedeli = dto.GuvenceBedeli,
        BaslangicTarihi = dto.BaslangicTarihi != default 
            ? DateOnly.FromDateTime(dto.BaslangicTarihi) 
            : DateOnly.FromDateTime(DateTime.UtcNow),
        Durum = SozlesmeDurumu.AKTIF,
        CreatedAt = DateTime.UtcNow
    };

    _context.Sozlesmelers.Add(yeniSozlesme);
    await _context.SaveChangesAsync();

    string isEmriPrefix = $"IE-{DateTime.UtcNow:yyyyMM}-";
    var sonIsEmri = await _context.IsEmirleris
        .Where(x => x.IsEmriNo.StartsWith(isEmriPrefix))
        .OrderByDescending(x => x.IsEmriNo)
        .FirstOrDefaultAsync();

    int isEmriSira = 1;
    if (sonIsEmri != null && int.TryParse(sonIsEmri.IsEmriNo.Substring(sonIsEmri.IsEmriNo.Length - 4), out int iSira))
        isEmriSira = iSira + 1;

    var otoAcmaIsEmri = new IsEmirleri
    {
        IsEmriNo = $"{isEmriPrefix}{isEmriSira:D4}",
        TuketimNoktasiId = yeniSozlesme.TuketimNoktasiId,
        Tip = IsEmriTipi.ACMA,
        Oncelik = "YUKSEK", 
        Durum = IsEmriDurumu.ACIK,
        CreatedAt = DateTime.UtcNow
    };

    _context.IsEmirleris.Add(otoAcmaIsEmri);
    await _context.SaveChangesAsync();

    return Ok(new { 
        message = "Sözleşme başarıyla oluşturuldu ve otomatik olarak ACMA iş emri oluşturuldu!", 
        sozlesmeNo = yeniSozlesmeNo,
        isEmriNo = otoAcmaIsEmri.IsEmriNo
    });
}

    // PUT: api/Sozlesmeler/5
    /// <summary>
    /// Mevcut bir sözleşmenin bilgilerini günceller.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> PutSozlesme(long id, Sozlesmeler sozlesme)
    {
        if (id != sozlesme.SozlesmeId)
        {
            return BadRequest(new
            {
                message = "Sözleşme Id uyuşmuyor."
            });
        }

        var mevcut = await _context.Sozlesmelers.FindAsync(id);

        if (mevcut == null)
        {
            return NotFound(new
            {
                message = "Sözleşme bulunamadı."
            });
        }

        if (mevcut.AboneId != sozlesme.AboneId)
        {
            var yeniAbone = await _context.Abonelers.FirstOrDefaultAsync(a => a.AboneId == sozlesme.AboneId);
            if (yeniAbone == null || yeniAbone.Status != "AKTIF")
            {
                return BadRequest(new { message = "Sözleşme sadece aktif abonelere atanabilir veya abone bulunamadı." });
            }
        }

        mevcut.SozlesmeNo = sozlesme.SozlesmeNo;
        mevcut.AboneId = sozlesme.AboneId;
        mevcut.TuketimNoktasiId = sozlesme.TuketimNoktasiId;
        mevcut.TarifeId = sozlesme.TarifeId;
        mevcut.SozlesmeTipi = sozlesme.SozlesmeTipi;
        mevcut.BaslangicTarihi = sozlesme.BaslangicTarihi;
        mevcut.BitisTarihi = sozlesme.BitisTarihi;
        mevcut.GuvenceBedeli = sozlesme.GuvenceBedeli;
        mevcut.Durum = System.Enum.Parse<SozlesmeDurumu>(sozlesme.Durum.ToString());
        mevcut.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // SILME: api/Sozlesmeler/5
    /// <summary>
    /// Bir sözleşmeyi iptal eder ve otomatik olarak 'Kesme' iş emri başlatır.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> SILMESozlesme(long id)
    {
        var sozlesme = await _context.Sozlesmelers.FindAsync(id);

        if (sozlesme == null)
            return NotFound(new { message = "Sözleşme bulunamadı." });

        bool faturaVar = await _context.Faturas.AnyAsync(x => x.SozlesmeId == id);
        if (faturaVar)
            return BadRequest(new { message = "Bu sözleşmeye ait fatura var, borcu ödenmeden silinemez." });

        long mekanId = sozlesme.TuketimNoktasiId;

        _context.Sozlesmelers.Remove(sozlesme);
        await _context.SaveChangesAsync();

        string isEmriPrefix = $"IE-{DateTime.UtcNow:yyyyMM}-";
        var sonIsEmri = await _context.IsEmirleris
            .Where(x => x.IsEmriNo.StartsWith(isEmriPrefix))
            .OrderByDescending(x => x.IsEmriNo)
            .FirstOrDefaultAsync();

        int isEmriSira = 1;
        if (sonIsEmri != null && int.TryParse(sonIsEmri.IsEmriNo.Substring(sonIsEmri.IsEmriNo.Length - 4), out int iSira))
            isEmriSira = iSira + 1;

        var otoKesmeIsEmri = new IsEmirleri
        {
            IsEmriNo = $"{isEmriPrefix}{isEmriSira:D4}",
            TuketimNoktasiId = mekanId,
            Tip = IsEmriTipi.KESME,
            Oncelik = "YUKSEK", 
            Durum = IsEmriDurumu.ACIK,
            CreatedAt = DateTime.UtcNow
        };

        _context.IsEmirleris.Add(otoKesmeIsEmri);
        await _context.SaveChangesAsync();

        return Ok(new { 
            message = "Sözleşme başarıyla silindi ve otomatik olarak KESME iş emri oluşturuldu!",
            kesmeIsEmriNo = otoKesmeIsEmri.IsEmriNo
        });
    }

    /// <summary>
/// Geliştirme ortamı için rastgele (fake) sözleşmeler üretir.
/// </summary>
[HttpPost("generate-fake-sozlesmeler")]
public async Task<IActionResult> GenerateFakeSozlesmeler()
{

    var aboneler = await _context.Abonelers.ToListAsync();
    var mekanlar = await _context.TuketimNoktasis.ToListAsync();
    var tarifeler = await _context.Tarifelers.ToListAsync();

    if (!aboneler.Any() || !mekanlar.Any() || !tarifeler.Any())
        return BadRequest(new { message = "Yeterli veri yok!" });

    int islemKapasitesi = Math.Min(aboneler.Count, mekanlar.Count);
    int basariliSozlesme = 0;

    string sozlesmePrefix = $"SOZ-{DateTime.UtcNow:yyyy-MM}-";
    string isEmriPrefix = $"IE-{DateTime.UtcNow:yyyyMM}-";

    int sozlesmeSira = 1;
    var sonSozlesme = await _context.Sozlesmelers.OrderByDescending(x => x.SozlesmeNo).FirstOrDefaultAsync(x => x.SozlesmeNo.StartsWith(sozlesmePrefix));
    if (sonSozlesme != null && int.TryParse(sonSozlesme.SozlesmeNo.Substring(sonSozlesme.SozlesmeNo.Length - 4), out int sSira))
        sozlesmeSira = sSira + 1;

    int isEmriSira = 1;
    var sonIsEmri = await _context.IsEmirleris.OrderByDescending(x => x.IsEmriNo).FirstOrDefaultAsync(x => x.IsEmriNo.StartsWith(isEmriPrefix));
    if (sonIsEmri != null && int.TryParse(sonIsEmri.IsEmriNo.Substring(sonIsEmri.IsEmriNo.Length - 4), out int iSira))
        isEmriSira = iSira + 1;

    var yeniSozlesmeler = new List<Sozlesmeler>();
    var yeniIsEmirleri = new List<IsEmirleri>();
    var random = new Random();

    for (int i = 0; i < islemKapasitesi; i++)
    {
        var secilenTarife = tarifeler[random.Next(tarifeler.Count)];

        var yeniSozlesme = new Sozlesmeler
        {
            SozlesmeNo = $"{sozlesmePrefix}{(sozlesmeSira + i):D4}",
            AboneId = aboneler[i].AboneId,
            TuketimNoktasiId = mekanlar[i].TuketimNoktasiId,
            TarifeId = secilenTarife.TarifeId,
            SozlesmeTipi = secilenTarife.TarifeAdi.Contains("Ticarethane") ? "TICARET" : "MESKEN",
            GuvenceBedeli = secilenTarife.TarifeAdi.Contains("Ticarethane") ? 4500.0m : 1500.0m,
            BaslangicTarihi = DateOnly.FromDateTime(DateTime.UtcNow),
            Durum = SozlesmeDurumu.AKTIF,
            CreatedAt = DateTime.UtcNow
        };
        yeniSozlesmeler.Add(yeniSozlesme);

        var otoAcmaIsEmri = new IsEmirleri
        {
            IsEmriNo = $"{isEmriPrefix}{(isEmriSira + i):D4}",
            TuketimNoktasiId = mekanlar[i].TuketimNoktasiId,
            Tip = IsEmriTipi.YENI_BAGLANTI,
            Oncelik = "YUKSEK", 
            Durum = IsEmriDurumu.ACIK,
            CreatedAt = DateTime.UtcNow
        };
        yeniIsEmirleri.Add(otoAcmaIsEmri);

        basariliSozlesme++;
    }

    _context.Sozlesmelers.AddRange(yeniSozlesmeler);
    _context.IsEmirleris.AddRange(yeniIsEmirleri);
    await _context.SaveChangesAsync();

    return Ok(new 
    { 
        message = $"{basariliSozlesme} sahte sözleşme başarıyla oluşturuldu ve her biri için otomatik olarak YENI_BAGLANTI iş emri oluşturuldu!", 
    });
}
}

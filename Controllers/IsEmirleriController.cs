using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KcetasAboneApi.Models;
using Microsoft.AspNetCore.Authorization;
using KcetasAboneApi.Models.Dtos;

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
    public async Task<ActionResult<IEnumerable<IsEmirleri>>> GetIsEmirleri()
    {
        return await _context.IsEmirleris
            .OrderBy(i => i.IsEmriId)
            .ToListAsync();
    }

    // GET: api/IsEmirleri/5
    [HttpGet("{id}")]
    public async Task<ActionResult<IsEmirleri>> GetIsEmri(long id)
    {
        var isEmri = await _context.IsEmirleris.FindAsync(id);

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

    if (dto.SayacId != null)
    {
        if (!await _context.Sayaclars.AnyAsync(x => x.SayacId == dto.SayacId))
            return BadRequest(new { message = "Sayaç bulunamadı." });
    }

    if (dto.AtananKullaniciId != null)
    {
        if (!await _context.Kullanicilars.AnyAsync(x => x.KullaniciId == dto.AtananKullaniciId))
            return BadRequest(new { message = "Kullanıcı bulunamadı." });
    }

    var buAykiSayi = await _context.IsEmirleris
        .CountAsync(x => x.CreatedAt.Year == DateTime.UtcNow.Year && x.CreatedAt.Month == DateTime.UtcNow.Month);

    string yeniIsEmriNo = $"IE-{DateTime.UtcNow:yyyyMM}-{(buAykiSayi + 1).ToString("D4")}";

    var yeniIsEmri = new IsEmirleri
    {
        IsEmriNo = yeniIsEmriNo,
        TuketimNoktasiId = dto.TuketimNoktasiId,
        SayacId = dto.SayacId,
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
}

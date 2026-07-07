using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KcetasAboneApi.Models;
using Microsoft.AspNetCore.Authorization;

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
    public async Task<ActionResult<IsEmirleri>> PostIsEmri(IsEmirleri isEmri)
    {
        if (await _context.IsEmirleris.AnyAsync(x => x.IsEmriNo == isEmri.IsEmriNo))
        {
            return BadRequest(new
            {
                message = "Bu iş emri numarası zaten kayıtlı."
            });
        }

        // Tüketim Noktası kontrolü
        if (!await _context.TuketimNoktasis.AnyAsync(x => x.TuketimNoktasiId == isEmri.TuketimNoktasiId))
        {
            return BadRequest(new
            {
                message = "Geçersiz tüketim noktası."
            });
        }

        // Sayaç kontrolü
        if (isEmri.SayacId != null)
        {
            if (!await _context.Sayaclars.AnyAsync(x => x.SayacId == isEmri.SayacId))
            {
                return BadRequest(new
                {
                    message = "Sayaç bulunamadı."
                });
            }
        }

        // Kullanıcı kontrolü
        if (isEmri.AtananKullaniciId != null)
        {
            if (!await _context.Kullanicilars.AnyAsync(x => x.KullaniciId == isEmri.AtananKullaniciId))
            {
                return BadRequest(new
                {
                    message = "Kullanıcı bulunamadı."
                });
            }
        }

        isEmri.CreatedAt = DateTime.UtcNow;

        _context.IsEmirleris.Add(isEmri);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetIsEmri),
            new { id = isEmri.IsEmriId }, isEmri);
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

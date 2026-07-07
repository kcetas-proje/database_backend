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
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Sozlesmeler>>> GetSozlesmeler()
    {
        return await _context.Sozlesmelers
            .OrderBy(s => s.SozlesmeId)
            .ToListAsync();
    }

    // GET: api/Sozlesmeler/5
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

    // POST: api/Sozlesmeler
    [HttpPost]
public async Task<ActionResult<Sozlesmeler>> PostSozlesme(SozlesmeCreateDto dto)
{

    if (!await _context.Abonelers.AnyAsync(x => x.AboneId == dto.AboneId))
        return BadRequest(new { message = "Abone bulunamadı." });

    if (!await _context.TuketimNoktasis.AnyAsync(x => x.TuketimNoktasiId == dto.TuketimNoktasiId))
        return BadRequest(new { message = "Tüketim noktası bulunamadı." });

    if (!await _context.Tarifelers.AnyAsync(x => x.TarifeId == dto.TarifeId))
        return BadRequest(new { message = "Tarife bulunamadı." });

    var count = await _context.Sozlesmelers.CountAsync();
    string yeniSozlesmeNo = $"SOZ-{DateTime.UtcNow:yyyy-MM}-{ (count + 1).ToString("D4") }";

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
        Durum = "AKTIF",
        CreatedAt = DateTime.UtcNow
    };

    _context.Sozlesmelers.Add(yeniSozlesme);
    await _context.SaveChangesAsync();

    return Ok(new { message = "Sözleşme başarıyla kuruldu!", no = yeniSozlesmeNo });
}

    // PUT: api/Sozlesmeler/5
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

        mevcut.SozlesmeNo = sozlesme.SozlesmeNo;
        mevcut.AboneId = sozlesme.AboneId;
        mevcut.TuketimNoktasiId = sozlesme.TuketimNoktasiId;
        mevcut.TarifeId = sozlesme.TarifeId;
        mevcut.SozlesmeTipi = sozlesme.SozlesmeTipi;
        mevcut.BaslangicTarihi = sozlesme.BaslangicTarihi;
        mevcut.BitisTarihi = sozlesme.BitisTarihi;
        mevcut.GuvenceBedeli = sozlesme.GuvenceBedeli;
        mevcut.Durum = sozlesme.Durum;
        mevcut.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/Sozlesmeler/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSozlesme(long id)
    {
        var sozlesme = await _context.Sozlesmelers.FindAsync(id);

        if (sozlesme == null)
        {
            return NotFound(new
            {
                message = "Sözleşme bulunamadı."
            });
        }

        // Bu sözleşmeye ait fatura var mı?
        bool faturaVar = await _context.Faturas
            .AnyAsync(x => x.SozlesmeId == id);

        if (faturaVar)
        {
            return BadRequest(new
            {
                message = "Bu sözleşmeye ait fatura bulunduğu için silinemez."
            });
        }

        _context.Sozlesmelers.Remove(sozlesme);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KcetasAboneApi.Models;

namespace KcetasAboneApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TuketimNoktasiController : ControllerBase
{
    private readonly KcetasAboneContext _context;

    public TuketimNoktasiController(KcetasAboneContext context)
    {
        _context = context;
    }

    // GET
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TuketimNoktasi>>> GetTuketimNoktalari()
    {
        return await _context.TuketimNoktasis
            .Include(x => x.Ilce)
            .OrderBy(x => x.TuketimNoktasiId)
            .ToListAsync();
    }

    // GET BY ID
    [HttpGet("{id}")]
    public async Task<ActionResult<TuketimNoktasi>> GetTuketimNoktasi(long id)
    {
        var nokta = await _context.TuketimNoktasis
            .Include(x => x.Ilce)
            .FirstOrDefaultAsync(x => x.TuketimNoktasiId == id);

        if (nokta == null)
            return NotFound(new { message = "Tüketim noktası bulunamadı." });

        return nokta;
    }

    // POST
    [HttpPost]
    public async Task<ActionResult<TuketimNoktasi>> PostTuketimNoktasi(TuketimNoktasi nokta)
    {
        if (await _context.TuketimNoktasis.AnyAsync(x => x.TekilKod == nokta.TekilKod))
            return BadRequest(new { message = "Tekil kod zaten kayıtlı." });

        if (!await _context.Ilces.AnyAsync(x => x.IlceId == nokta.IlceId))
            return BadRequest(new { message = "İlçe bulunamadı." });

        nokta.CreatedAt = DateTime.UtcNow;

        _context.TuketimNoktasis.Add(nokta);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTuketimNoktasi),
            new { id = nokta.TuketimNoktasiId }, nokta);
    }

    // PUT
    [HttpPut("{id}")]
    public async Task<IActionResult> PutTuketimNoktasi(long id, TuketimNoktasi nokta)
    {
        if (id != nokta.TuketimNoktasiId)
            return BadRequest();

        var mevcut = await _context.TuketimNoktasis.FindAsync(id);

        if (mevcut == null)
            return NotFound();

        mevcut.IlceId = nokta.IlceId;
        mevcut.Mahalle = nokta.Mahalle;
        mevcut.BinaNo = nokta.BinaNo;
        mevcut.BagimsizBolumNo = nokta.BagimsizBolumNo;
        mevcut.AcikAdres = nokta.AcikAdres;
        mevcut.KoordinatLat = nokta.KoordinatLat;
        mevcut.KoordinatLon = nokta.KoordinatLon;
        mevcut.BaglantiGucuKw = nokta.BaglantiGucuKw;
        mevcut.TuketiciGrubu = nokta.TuketiciGrubu;
        mevcut.BaglantiDurumu = nokta.BaglantiDurumu;
        mevcut.Status = nokta.Status;
        mevcut.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTuketimNoktasi(long id)
    {
        var nokta = await _context.TuketimNoktasis.FindAsync(id);

        if (nokta == null)
            return NotFound();

        bool kullaniliyor = await _context.Sozlesmelers
            .AnyAsync(x => x.TuketimNoktasiId == id);

        if (kullaniliyor)
            return BadRequest(new
            {
                message = "Bu tüketim noktasına ait sözleşme bulunduğu için silinemez."
            });

        _context.TuketimNoktasis.Remove(nokta);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

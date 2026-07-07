using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KcetasAboneApi.Models;

namespace KcetasAboneApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SayaclarController : ControllerBase
{
    private readonly KcetasAboneContext _context;

    public SayaclarController(KcetasAboneContext context)
    {
        _context = context;
    }

    // GET
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Sayaclar>>> GetSayaclar()
    {
        return await _context.Sayaclars
            .Include(x => x.TuketimNoktasi)
            .OrderBy(x => x.SayacId)
            .ToListAsync();
    }

    // GET BY ID
    [HttpGet("{id}")]
    public async Task<ActionResult<Sayaclar>> GetSayac(long id)
    {
        var sayac = await _context.Sayaclars
            .Include(x => x.TuketimNoktasi)
            .FirstOrDefaultAsync(x => x.SayacId == id);

        if (sayac == null)
            return NotFound(new
            {
                message = "Sayaç bulunamadı."
            });

        return sayac;
    }

    // POST
    [HttpPost]
    public async Task<ActionResult<Sayaclar>> PostSayac(Sayaclar sayac)
    {
        if (await _context.Sayaclars.AnyAsync(x => x.SeriNo == sayac.SeriNo))
            return BadRequest(new
            {
                message = "Seri numarası zaten kayıtlı."
            });

        if (sayac.TuketimNoktasiId != null)
        {
            bool noktaVar = await _context.TuketimNoktasis
                .AnyAsync(x => x.TuketimNoktasiId == sayac.TuketimNoktasiId);

            if (!noktaVar)
                return BadRequest(new
                {
                    message = "Tüketim noktası bulunamadı."
                });
        }

        sayac.CreatedAt = DateTime.UtcNow;

        _context.Sayaclars.Add(sayac);

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSayac),
            new { id = sayac.SayacId }, sayac);
    }

    // PUT
    [HttpPut("{id}")]
    public async Task<IActionResult> PutSayac(long id, Sayaclar sayac)
    {
        if (id != sayac.SayacId)
            return BadRequest();

        var mevcut = await _context.Sayaclars.FindAsync(id);

        if (mevcut == null)
            return NotFound();

        mevcut.SeriNo = sayac.SeriNo;
        mevcut.TuketimNoktasiId = sayac.TuketimNoktasiId;
        mevcut.Marka = sayac.Marka;
        mevcut.Model = sayac.Model;
        mevcut.Faz = sayac.Faz;
        mevcut.Carpan = sayac.Carpan;
        mevcut.MuhurNo = sayac.MuhurNo;
        mevcut.Durum = sayac.Durum;
        mevcut.SonOkumaTarihi = sayac.SonOkumaTarihi;
        mevcut.Status = sayac.Status;
        mevcut.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSayac(long id)
    {
        var sayac = await _context.Sayaclars.FindAsync(id);

        if (sayac == null)
            return NotFound();

        bool kullaniliyor = await _context.EndeksOkumas
            .AnyAsync(x => x.SayacId == id);

        if (kullaniliyor)
            return BadRequest(new
            {
                message = "Bu sayaç için endeks okuma bulunduğu için silinemez."
            });

        _context.Sayaclars.Remove(sayac);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}

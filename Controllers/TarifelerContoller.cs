using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KcetasAboneApi.Models;
using Microsoft.AspNetCore.Authorization;

namespace KcetasAboneApi.Controllers;

//[Authorize(Roles = "1")]
[Route("api/[controller]")]
[ApiController]
public class TarifelerController : ControllerBase
{
    private readonly AppDbContext _context; // KcetasAboneContext yerine bunu yazdık

    public TarifelerController(AppDbContext context)
    {
        _context = context;
    }
    
    // ... (Arkadaşının yazdığı diğer GET, POST, PUT, DELETE metodları kusursuz, aşağısı aynı kalacak)

    // GET: api/Tarifeler
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Tarifeler>>> GetTarifeler()
    {
        return await _context.Tarifelers
            .OrderBy(t => t.TarifeId)
            .ToListAsync();
    }

    // GET: api/Tarifeler/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Tarifeler>> GetTarife(long id)
    {
        var tarife = await _context.Tarifelers.FindAsync(id);

        if (tarife == null)
        {
            return NotFound(new
            {
                message = "Tarife bulunamadı."
            });
        }

        return tarife;
    }

    // POST: api/Tarifeler
    [HttpPost]
    public async Task<ActionResult<Tarifeler>> PostTarife(Tarifeler tarife)
    {
        bool kodVar = await _context.Tarifelers
            .AnyAsync(t => t.TarifeKodu == tarife.TarifeKodu);

        if (kodVar)
        {
            return BadRequest(new
            {
                message = "Bu tarife kodu zaten kayıtlı."
            });
        }

        tarife.CreatedAt = DateTime.UtcNow;

        _context.Tarifelers.Add(tarife);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetTarife),
            new { id = tarife.TarifeId },
            tarife);
    }

    // PUT: api/Tarifeler/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutTarife(long id, Tarifeler tarife)
    {
        if (id != tarife.TarifeId)
        {
            return BadRequest(new
            {
                message = "Tarife Id uyuşmuyor."
            });
        }

        var mevcutTarife = await _context.Tarifelers.FindAsync(id);

        if (mevcutTarife == null)
        {
            return NotFound(new
            {
                message = "Tarife bulunamadı."
            });
        }

        bool kodVar = await _context.Tarifelers.AnyAsync(t =>
            t.TarifeKodu == tarife.TarifeKodu &&
            t.TarifeId != id);

        if (kodVar)
        {
            return BadRequest(new
            {
                message = "Bu tarife kodu başka bir tarifede kullanılıyor."
            });
        }

        mevcutTarife.TarifeKodu = tarife.TarifeKodu;
        mevcutTarife.TarifeAdi = tarife.TarifeAdi;
        mevcutTarife.GunduzBirimFiyat = tarife.GunduzBirimFiyat;
        mevcutTarife.PuantBirimFiyat = tarife.PuantBirimFiyat;
        mevcutTarife.GeceBirimFiyat = tarife.GeceBirimFiyat;
        mevcutTarife.KdvOrani = tarife.KdvOrani;
        mevcutTarife.DagitimBedeli = tarife.DagitimBedeli;
        mevcutTarife.Aktif = tarife.Aktif;
        mevcutTarife.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/Tarifeler/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTarife(long id)
    {
        var tarife = await _context.Tarifelers.FindAsync(id);

        if (tarife == null)
        {
            return NotFound(new
            {
                message = "Tarife bulunamadı."
            });
        }

        bool kullaniliyor = await _context.Sozlesmelers
            .AnyAsync(s => s.TarifeId == id);

        if (kullaniliyor)
        {
            return BadRequest(new
            {
                message = "Bu tarife sözleşmelerde kullanıldığı için silinemez."
            });
        }

        _context.Tarifelers.Remove(tarife);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}

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
    private readonly AppDbContext _context; 

    public TarifelerController(AppDbContext context)
    {
        _context = context;
    }
    

    // GET: api/Tarifeler
    /// <summary>
    /// Sistemdeki tüm tarifeleri liste halinde getirir.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Tarifeler>>> GetTarifeler()
    {
        return await _context.Tarifelers
            .OrderBy(t => t.TarifeId)
            .ToListAsync();
    }

    // GET: api/Tarifeler/5
    /// <summary>
    /// ID'si verilen tarifenin detaylarını getirir.
    /// </summary>
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
    /// <summary>
    /// Sisteme yeni bir elektrik tarifesi ekler.
    /// </summary>
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
    /// <summary>
    /// Mevcut bir tarifenin birim fiyat, vergi ve oran bilgilerini günceller.
    /// </summary>
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

    // SILME: api/Tarifeler/5
    /// <summary>
    /// Bir tarifeyi sistemden siler. Mevcut bir sözleşmede kullanılıyorsa silinemez.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> SILMETarife(long id)
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

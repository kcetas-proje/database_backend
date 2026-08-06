using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KcetasAboneApi.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace KcetasAboneApi.Controllers;

//[Authorize(Roles = "1, 6")]
[ApiController]
[Route("api/[controller]")]
public class FaturaKalemiController : ControllerBase
{
    private readonly AppDbContext _context;

    public FaturaKalemiController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Tüm fatura kalemlerini (detayları) getirir.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FaturaKalemi>>> Get()
    {
        return await _context.FaturaKalemis
            .Include(x => x.Fatura)
            .ToListAsync();
    }

    /// <summary>
    /// Belirli bir fatura kaleminin detaylarını getirir.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<FaturaKalemi>> Get(long id)
    {
        var kalem = await _context.FaturaKalemis
            .Include(x => x.Fatura)
            .FirstOrDefaultAsync(x => x.FaturaKalemId == id);

        if (kalem == null)
            return NotFound();

        return kalem;
    }

    /// <summary>
    /// Faturaya yeni bir kalem (tüketim, vergi, ceza vb.) ekler.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<FaturaKalemi>> Post([FromBody] FaturaKalemiCreateDto dto)
    {
        var yeniKalem = new FaturaKalemi
        {
            FaturaId = dto.FaturaId,
            KalemTipi = dto.KalemTipi,
            Aciklama = dto.Aciklama ?? "Sistem tarafından otomatik oluşturuldu",
            Miktar = dto.Miktar,
            BirimFiyat = dto.BirimFiyat,
            Tutar = dto.Tutar,
            CreatedAt = DateTime.UtcNow
        };

        _context.FaturaKalemis.Add(yeniKalem);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = yeniKalem.FaturaKalemId }, yeniKalem);
    }

    /// <summary>
    /// Mevcut bir fatura kaleminin miktar, fiyat veya açıklama bilgisini günceller.
    /// </summary>
    [HttpPut("{id}")]
public async Task<IActionResult> Update(long id, [FromBody] FaturaKalemiUpdateDto dto)
{
    var kalem = await _context.FaturaKalemis.FindAsync(id);
    if (kalem == null) return NotFound();

    kalem.Aciklama = dto.Aciklama;
    kalem.Miktar = dto.Miktar;
    kalem.BirimFiyat = dto.BirimFiyat;
    kalem.Tutar = dto.Miktar * dto.BirimFiyat;

    await _context.SaveChangesAsync();
    return NoContent();
}

    /// <summary>
    /// Bir fatura kalemini siler.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> SILME(long id)
    {
        var kalem = await _context.FaturaKalemis.FindAsync(id);

        if (kalem == null)
            return NotFound();

        _context.FaturaKalemis.Remove(kalem);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

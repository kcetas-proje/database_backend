using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KcetasAboneApi.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KcetasAboneApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EndeksOkumaController : ControllerBase
{
    private readonly AppDbContext _context;

    public EndeksOkumaController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EndeksOkuma>>> GetAll()
    {
        return await _context.EndeksOkumas
            .Include(x => x.Sayac)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EndeksOkuma>> Get(long id)
    {
        var okuma = await _context.EndeksOkumas
            .Include(x => x.Sayac)
            .FirstOrDefaultAsync(x => x.OkumaId == id);

        if (okuma == null)
            return NotFound();

        return okuma;
    }

    [HttpPost]
    public async Task<ActionResult<EndeksOkuma>> Post([FromBody] EndeksOkumaCreateDto dto)
    {
        var yeniEndeks = new EndeksOkuma
        {
            SayacId = dto.SayacId,
            IsEmriId = dto.IsEmriId,
            SozlesmeId = dto.SozlesmeId,
            YeniEndeks = dto.YeniEndeks,
            OncekiEndeks = dto.OncekiEndeks,
            
            OkumaTipi = string.IsNullOrEmpty(dto.OkumaTipi) ? "RUTIN_DONEM" : dto.OkumaTipi,
            OkumaKaynagi = string.IsNullOrEmpty(dto.OkumaKaynagi) ? "MANUEL" : dto.OkumaKaynagi,
            Donem = string.IsNullOrEmpty(dto.Donem) ? DateTime.Now.ToString("yyyy-MM") : dto.Donem,
            OkumaZamani = dto.OkumaZamani ?? DateTime.UtcNow,
            KullaniciId = dto.KullaniciId,

            DogrulamaDurumu = "DOGRULAMA_BEKLIYOR", 
            AnomaliMi = false, 
            Status = "AKTIF",
            CreatedAt = DateTime.UtcNow
        };

        _context.EndeksOkumas.Add(yeniEndeks);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = yeniEndeks.OkumaId }, yeniEndeks);
    }

    [HttpPut("{id}")]
public async Task<IActionResult> Update(long id, [FromBody] EndeksOkumaUpdateDto dto)
{
    var endeks = await _context.EndeksOkumas.FindAsync(id);
    if (endeks == null) return NotFound();

    endeks.YeniEndeks = dto.Deger; 
    endeks.OkumaTipi = dto.OkumaTipi;

    await _context.SaveChangesAsync();
    return NoContent();
}

    [HttpDelete("{id}")]
public async Task<IActionResult> Delete(long id)
{
    var endeks = await _context.EndeksOkumas.FindAsync(id);
    if (endeks == null) return NotFound();

    endeks.Status = "PASIF";

    await _context.SaveChangesAsync();
    return NoContent(); 
}
}

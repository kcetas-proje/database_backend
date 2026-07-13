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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FaturaKalemi>>> Get()
    {
        return await _context.FaturaKalemis
            .Include(x => x.Fatura)
            .ToListAsync();
    }

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

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var kalem = await _context.FaturaKalemis.FindAsync(id);

        if (kalem == null)
            return NotFound();

        _context.FaturaKalemis.Remove(kalem);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("generate-eksik-kalemler")]
    public async Task<IActionResult> GenerateEksikKalemler()
    {
        var faturalar = await _context.Faturas
            .Include(f => f.FaturaKalemis)
            .Where(f => !f.FaturaKalemis.Any())
            .ToListAsync();

        if (!faturalar.Any())
            return BadRequest(new { message = "Fatura kalemi olmayan fatura bulunamadı." });

        var yeniKalemler = new List<FaturaKalemi>();

        foreach (var fatura in faturalar)
        {

            decimal tuketim = (fatura.TuketimKwh.HasValue && fatura.TuketimKwh.Value > 0) ? fatura.TuketimKwh.Value : 1m;

            if (fatura.EnerjiBedeli > 0)
            {
                yeniKalemler.Add(new FaturaKalemi
                {
                    FaturaId = fatura.FaturaId,
                    KalemTipi = "ENERJI", 
                    Aciklama = "Gündüz/Puant/Gece Aktif Enerji Tüketimi",
                    Miktar = tuketim,
                    BirimFiyat = Math.Round(fatura.EnerjiBedeli / tuketim, 4),
                    Tutar = fatura.EnerjiBedeli,
                    CreatedAt = DateTime.UtcNow
                });
            }

            if (fatura.DagitimBedeli > 0)
            {
                yeniKalemler.Add(new FaturaKalemi
                {
                    FaturaId = fatura.FaturaId,
                    KalemTipi = "DAGITIM",
                    Aciklama = "Dağıtım Sistemi Kullanım Bedeli",
                    Miktar = tuketim,
                    BirimFiyat = Math.Round(fatura.DagitimBedeli / tuketim, 4),
                    Tutar = fatura.DagitimBedeli,
                    CreatedAt = DateTime.UtcNow
                });
            }

            if (fatura.VergiFonToplam > 0)
            {
                yeniKalemler.Add(new FaturaKalemi
                {
                    FaturaId = fatura.FaturaId,
                    KalemTipi = "VERGI_FON",
                    Aciklama = "%20 KDV ve Diğer Yasal Fonlar",
                    Miktar = 1m, 
                    BirimFiyat = fatura.VergiFonToplam,
                    Tutar = fatura.VergiFonToplam,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        _context.FaturaKalemis.AddRange(yeniKalemler);
        await _context.SaveChangesAsync();

        return Ok(new 
        { 
            message = $"{yeniKalemler.Count} adet eksik fatura kalemi başarıyla oluşturuldu.",
            eklenenKalemSayisi = yeniKalemler.Count
        });
    }
}

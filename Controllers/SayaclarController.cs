using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KcetasAboneApi.Models;
using Microsoft.AspNetCore.Authorization;

namespace KcetasAboneApi.Controllers;

//[Authorize(Roles = "1, 5")]
[Route("api/[controller]")]
[ApiController]
public class SayaclarController : ControllerBase
{
    private readonly AppDbContext _context;

    public SayaclarController(AppDbContext context)
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
public async Task<ActionResult<Sayaclar>> PostSayac(SayacCreateDto dto)
{
    // 1. Kapıdaki Bodyguard Kontrolleri
    if (await _context.Sayaclars.AnyAsync(x => x.SeriNo == dto.SeriNo))
        return BadRequest(new { message = "Bu seri numarası zaten kayıtlı." });

    if (dto.TuketimNoktasiId != null)
    {
        bool noktaVar = await _context.TuketimNoktasis
            .AnyAsync(x => x.TuketimNoktasiId == dto.TuketimNoktasiId);

        if (!noktaVar)
            return BadRequest(new { message = "Tüketim noktası bulunamadı." });
    }

    var yeniSayac = new Sayaclar
    {
        SeriNo = dto.SeriNo,
        TuketimNoktasiId = dto.TuketimNoktasiId,
        Marka = dto.Marka ?? "Bilinmiyor",
        Model = dto.Model ?? "Bilinmiyor",
        
        UretimYili = dto.UretimYili == 0 ? DateTime.UtcNow.Year : dto.UretimYili, 
        
        Faz = string.IsNullOrEmpty(dto.Faz) ? "TEK_FAZ" : dto.Faz,
        Carpan = dto.Carpan == 0 ? 1 : dto.Carpan,
        MuhurNo = dto.MuhurNo,
        Durum = string.IsNullOrEmpty(dto.Durum) ? "DEPODA" : dto.Durum,
        
        CreatedAt = DateTime.UtcNow,
        CreatedBy = 1 
    };

    _context.Sayaclars.Add(yeniSayac);
    await _context.SaveChangesAsync();

    return CreatedAtAction(nameof(GetSayac), new { id = yeniSayac.SayacId }, yeniSayac);
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

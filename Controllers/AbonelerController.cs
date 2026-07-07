using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KcetasAboneApi.Models;
using Microsoft.AspNetCore.Authorization;

namespace KcetasAboneApi.Controllers;

//[Authorize(Roles = "1,2")]
[Route("api/[controller]")]
[ApiController]
public class AbonelerController : ControllerBase
{
    private readonly AppDbContext _context;

    public AbonelerController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Aboneler
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Aboneler>>> GetAboneler()
    {
        return await _context.Abonelers
            .OrderBy(a => a.AboneId)
            .ToListAsync();
    }

    // GET: api/Aboneler/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Aboneler>> GetAbone(long id)
    {
        var abone = await _context.Abonelers.FindAsync(id);

        if (abone == null)
        {
            return NotFound(new
            {
                message = "Abone bulunamadı."
            });
        }

        return abone;
    }

    // POST: api/Aboneler
    [HttpPost]
    public async Task<ActionResult<Aboneler>> PostAbone(Aboneler abone)
    {
        // Abone numarası kontrolü
        if (await _context.Abonelers.AnyAsync(a => a.AboneNo == abone.AboneNo))
        {
            return BadRequest(new
            {
                message = "Bu abone numarası zaten kayıtlı."
            });
        }

        // TCKN kontrolü
        if (!string.IsNullOrWhiteSpace(abone.Tckn))
        {
            bool tcknVar = await _context.Abonelers.AnyAsync(a => a.Tckn == abone.Tckn);

            if (tcknVar)
            {
                return BadRequest(new
                {
                    message = "Bu TCKN sistemde kayıtlı."
                });
            }
        }

        // VKN kontrolü
        if (!string.IsNullOrWhiteSpace(abone.Vkn))
        {
            bool vknVar = await _context.Abonelers.AnyAsync(a => a.Vkn == abone.Vkn);

            if (vknVar)
            {
                return BadRequest(new
                {
                    message = "Bu VKN sistemde kayıtlı."
                });
            }
        }

        abone.CreatedAt = DateTime.UtcNow;

        _context.Abonelers.Add(abone);

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAbone),
            new { id = abone.AboneId }, abone);
    }

    // PUT: api/Aboneler/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutAbone(long id, Aboneler abone)
    {
        if (id != abone.AboneId)
        {
            return BadRequest(new
            {
                message = "Abone Id uyuşmuyor."
            });
        }

        var mevcutAbone = await _context.Abonelers.FindAsync(id);

        if (mevcutAbone == null)
        {
            return NotFound(new
            {
                message = "Abone bulunamadı."
            });
        }

        mevcutAbone.AboneNo = abone.AboneNo;
        mevcutAbone.AboneTipi = abone.AboneTipi;
        mevcutAbone.Ad = abone.Ad;
        mevcutAbone.Soyad = abone.Soyad;
        mevcutAbone.Unvan = abone.Unvan;
        mevcutAbone.Tckn = abone.Tckn;
        mevcutAbone.Vkn = abone.Vkn;
        mevcutAbone.Telefon = abone.Telefon;
        mevcutAbone.EPosta = abone.EPosta;
        mevcutAbone.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/Aboneler/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAbone(long id)
    {
        var abone = await _context.Abonelers.FindAsync(id);

        if (abone == null)
        {
            return NotFound(new
            {
                message = "Abone bulunamadı."
            });
        }

        // Bu aboneye ait sözleşme var mı?
        bool kullaniliyor = await _context.Sozlesmelers
            .AnyAsync(s => s.AboneId == id);

        if (kullaniliyor)
        {
            return BadRequest(new
            {
                message = "Bu aboneye ait sözleşme bulunduğu için silinemez."
            });
        }

        _context.Abonelers.Remove(abone);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}

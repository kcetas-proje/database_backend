using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KcetasAboneApi.Models;
using Microsoft.AspNetCore.Authorization;
using Bogus;

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
    public async Task<ActionResult<Aboneler>> PostAbone(AboneCreateDto dto)
    {
        string yeniAboneNo = $"ABN-{DateTime.Now:yyyyMMddHHmmss}";

        var yeniAbone = new Aboneler
        {
            AboneNo = yeniAboneNo, 
            AboneTipi = dto.AboneTipi,
            Ad = dto.Ad,
            Soyad = dto.Soyad,
            Tckn = dto.Tckn,
            Vkn = dto.Vkn,
            Unvan = dto.Unvan,
            Telefon = dto.Telefon,
            CreatedAt = DateTime.UtcNow
        };

        if (!string.IsNullOrWhiteSpace(yeniAbone.Tckn) && await _context.Abonelers.AnyAsync(a => a.Tckn == yeniAbone.Tckn))
            return BadRequest(new { message = "Bu TCKN sistemde kayıtlı." });

        if (!string.IsNullOrWhiteSpace(yeniAbone.Vkn) && await _context.Abonelers.AnyAsync(a => a.Vkn == yeniAbone.Vkn))
            return BadRequest(new { message = "Bu VKN sistemde kayıtlı." });

        // 4. Kayıt
        _context.Abonelers.Add(yeniAbone);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Abone başarıyla oluşturuldu!", aboneNo = yeniAboneNo });
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
    [HttpPost("generate-fake-aboneler")]
    public async Task<IActionResult> GenerateFakeAboneler()
    {
        var aboneFaker = new Faker<Aboneler>("tr") 
            .RuleFor(a => a.AboneNo, f => $"ABN-{DateTime.Now.AddSeconds(f.IndexGlobal):yyyyMMddHHmmss}")
            .RuleFor(a => a.Ad, f => f.Name.FirstName())
            .RuleFor(a => a.Soyad, f => f.Name.LastName())
            .RuleFor(a => a.Tckn, f => f.Random.Replace("###########"))
            .RuleFor(a => a.Telefon, f => f.Phone.PhoneNumber("05#########"))
            .RuleFor(a => a.EPosta, (f, a) => f.Internet.Email(a.Ad, a.Soyad).ToLower())

            .RuleFor(a => a.AboneTipi, "BIREYSEL")
            
            .RuleFor(a => a.CreatedAt, f => f.Date.Past(1).ToUniversalTime());

        var sahteAboneler = aboneFaker.Generate(50); 
        _context.Abonelers.AddRange(sahteAboneler);
        await _context.SaveChangesAsync();

        return Ok(new 
        { 
            message = "50 sahte abone başarıyla oluşturuldu.", 
            eklenenSayi = sahteAboneler.Count 
        });
} 
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KcetasAboneApi.Models;
using Microsoft.AspNetCore.Authorization;
using Bogus;

namespace KcetasAboneApi.Controllers;
using KcetasAboneApi.Models.Dtos;
using KcetasAboneApi.Services;

//[Authorize(Roles = "1,2")]
[Route("api/[controller]")]
[ApiController]
public class AbonelerController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IAboneService _aboneService;

    public AbonelerController(
        AppDbContext context,
        IAboneService aboneService)
    {
        _context = context;
        _aboneService = aboneService;
    }

    [HttpGet("All")]
    public async Task<ActionResult<IEnumerable<Aboneler>>> GetAllAboneler()
    {
        return await _context.Abonelers
            .AsNoTracking()
            .Where(a => a.Status == "AKTIF")
            .OrderBy(a => a.AboneId)
            .Take(500)
            .ToListAsync();
    }
    // GET: api/Aboneler
    [HttpGet]
    public async Task<IActionResult> GetAboneler([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] long? lastId = null, [FromQuery] int? limit = null)
    {
        // Web ekibinden gelen limit parametresi varsa onu pageSize olarak kullan
        if (limit.HasValue) pageSize = limit.Value;

        // Güvenlik kontrolleri: Sayfa 1'den, istenen kayıt sayısı 1'den küçük olamaz.
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100; 

        var totalCount = await _context.Abonelers.CountAsync(a => a.Status == "AKTIF");
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var query = _context.Abonelers.AsNoTracking().Where(a => a.Status == "AKTIF");

        if (lastId.HasValue && lastId.Value > 0)
        {
            query = query.Where(a => a.AboneId > lastId.Value);
        }
        else
        {
            query = query.Skip((page - 1) * pageSize);
        }

        var aboneler = await query
            .OrderBy(a => a.AboneId)
            .Take(pageSize)
            .Select(a => new AboneListDto
            {
                AboneId = a.AboneId,
                AboneNo = a.AboneNo,
                Ad = a.Ad,
                Soyad = a.Soyad,
                Unvan = a.Unvan,
                Tckn = a.Tckn,
                Vkn = a.Vkn,
                Telefon = a.Telefon,
                AboneTipi = a.AboneTipi
            })
            .ToListAsync();

        long? nextCursor = aboneler.Any() ? aboneler.Last().AboneId : null;


        var response = new PagedResultDto<AboneListDto>
        {
            TotalCount = totalCount,
            TotalPages = totalPages,
            CurrentPage = page,
            PageSize = pageSize,
            NextCursor = nextCursor,
            Data = aboneler
        };

        return Ok(response);
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

    // SILME: api/Aboneler/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> SILMEAbone(long id)
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

        abone.Status = "PASIF";
        abone.UpdatedAt = DateTime.UtcNow;

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

            .RuleFor(a => a.AboneTipi, AboneTipi.BIREYSEL)
            
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
    [HttpPost("abone-ve-tesisat-kaydet")]
    public async Task<IActionResult> AboneVeTesisatKaydet([FromBody] AboneVeTesisatCreateDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            // 1. ABONE OLUŞTURMA
            string yeniAboneNo = $"ABN-{DateTime.Now:yyyyMMddHHmmss}";
            
            var yeniAbone = new Aboneler
            {
                AboneNo = yeniAboneNo,
                Ad = dto.Ad,
                Soyad = dto.Soyad,
                Tckn = dto.TcNo,
                Telefon = dto.Telefon,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Abonelers.Add(yeniAbone);
            await _context.SaveChangesAsync();

            string prefix = $"TK-{DateTime.UtcNow:yyyyMM}-";
            int sira = 1;
            var sonNokta = await _context.TuketimNoktasis
                .Where(x => x.TekilKod.StartsWith(prefix))
                .OrderByDescending(x => x.TekilKod)
                .FirstOrDefaultAsync();

            if (sonNokta != null && int.TryParse(sonNokta.TekilKod.Substring(sonNokta.TekilKod.Length - 4), out int sonSira))
                sira = sonSira + 1;

            var yeniMekan = new TuketimNoktasi
            {
                TekilKod = $"{prefix}{sira:D4}",
                IlceId = dto.IlceId,
                Mahalle = dto.Mahalle,
                BinaNo = dto.BinaNo,
                BagimsizBolumNo = dto.BagimsizBolumNo,
                AcikAdres = dto.AcikAdres,
                BaglantiGucuKw = dto.BaglantiGucuKw,
                TuketiciGrubu = dto.TuketiciGrubu,
                BaglantiDurumu = BaglantiDurumu.PASIF,
                Status = "AKTIF",
                CreatedAt = DateTime.UtcNow
            };

            _context.TuketimNoktasis.Add(yeniMekan);
            await _context.SaveChangesAsync();

            string sozlesmePrefix = $"SOZ-{DateTime.UtcNow:yyyy-MM}-";
            int sozlesmeSira = 1;
            var sonSozlesme = await _context.Sozlesmelers
                .Where(x => x.SozlesmeNo.StartsWith(sozlesmePrefix))
                .OrderByDescending(x => x.SozlesmeNo)
                .FirstOrDefaultAsync();

            if (sonSozlesme != null && int.TryParse(sonSozlesme.SozlesmeNo.Substring(sonSozlesme.SozlesmeNo.Length - 4), out int sSira))
                sozlesmeSira = sSira + 1;

            var yeniSozlesme = new Sozlesmeler
            {
                SozlesmeNo = $"{sozlesmePrefix}{sozlesmeSira:D4}",
                AboneId = yeniAbone.AboneId,
                TuketimNoktasiId = yeniMekan.TuketimNoktasiId,
                TarifeId = dto.TuketiciGrubu == "TICARET" ? 3 : 1,
                SozlesmeTipi = dto.TuketiciGrubu,
                GuvenceBedeli = dto.TuketiciGrubu == "TICARET" ? 4500m : 1500m,
                BaslangicTarihi = DateOnly.FromDateTime(DateTime.UtcNow),
                Durum = SozlesmeDurumu.AKTIF,
                CreatedAt = DateTime.UtcNow
            };

            _context.Sozlesmelers.Add(yeniSozlesme);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return Ok(new 
            { 
                message = "Abone, Tesisat ve Sözleşme başarıyla oluşturuldu.", 
                aboneNo = yeniAbone.AboneNo,
                tekilKod = yeniMekan.TekilKod,
                sozlesmeNo = yeniSozlesme.SozlesmeNo
            });
        }
        catch (Exception ex)
        {

            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "Bir hata oluştu.", error = ex.Message });
        }
    }
    [HttpGet("arama")]
    public async Task<IActionResult> GetAboneler(
    [FromQuery] string? isim,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
    {
        if (page < 1)
            page = 1;

        if (pageSize < 1)
            pageSize = 10;

        if (pageSize > 100)
            pageSize = 100;

        var sonuc = await _aboneService.GetAboneler(
            isim,
            page,
            pageSize);

        return Ok(sonuc);
    }
    [HttpGet("{aboneId}/faturalar")]
    public async Task<IActionResult> GetAboneFaturalari(
    long aboneId,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
    {
        if (page < 1)
            page = 1;

        if (pageSize < 1)
            pageSize = 10;

        if (pageSize > 100)
            pageSize = 100;

        var sonuc = await _aboneService.GetAboneFaturalari(
            aboneId,
            page,
            pageSize);

        return Ok(sonuc);
    }
}

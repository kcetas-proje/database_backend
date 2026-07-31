using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KcetasAboneApi.Models;
using Microsoft.AspNetCore.Authorization;
using KcetasAboneApi.Models.Dtos;
using Bogus;


namespace KcetasAboneApi.Controllers;

//[Authorize(Roles = "1, 2")]
[Route("api/[controller]")]
[ApiController]
public class TuketimNoktasiController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<TuketimNoktasiController> _logger;

    public TuketimNoktasiController(AppDbContext context, ILogger<TuketimNoktasiController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET
   [HttpGet]
    public async Task<ActionResult<IEnumerable<TuketimNoktasi>>> GetTuketimNoktalari()
    {
        return await _context.TuketimNoktasis
            .Include(x => x.Ilce)
            .Include(x => x.Sozlesmelers)
                .ThenInclude(s => s.Abone)
            .OrderBy(x => x.TuketimNoktasiId)
            .ToListAsync();
    }

    [HttpGet("Paged")]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? q = null,
        [FromQuery] BaglantiDurumu? baglantiDurumu = null)
    {
        var query = _context.TuketimNoktasis
            .AsNoTracking()
            .AsQueryable();

        if (baglantiDurumu.HasValue)
        {
            query = query.Where(t => t.BaglantiDurumu == baglantiDurumu.Value);
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            var qLower = q.ToLower();
            bool isNumeric = long.TryParse(q, out long numQ);

            query = query.Where(t => 
                t.TekilKod.ToLower().Contains(qLower) || 
                t.AcikAdres.ToLower().Contains(qLower) ||
                t.Mahalle.ToLower().Contains(qLower) ||
                (isNumeric && t.TuketimNoktasiId == numQ)
            );
        }

        var total = await query.CountAsync();

        var data = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new
            {
                t.TuketimNoktasiId,
                t.TekilKod,
                t.IlceId,
                t.Mahalle,
                t.BinaNo,
                t.BagimsizBolumNo,
                t.AcikAdres,
                t.KoordinatLat,
                t.KoordinatLon,
                t.BaglantiGucuKw,
                t.TuketiciGrubu,
                BaglantiDurumu = t.BaglantiDurumu.ToString(),
                t.Status,
                t.CreatedAt,
                t.UpdatedAt
            })
            .ToListAsync();

        return Ok(new
        {
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            Data = data
        });
    }

    [HttpGet("GetWithDetails")]
    public async Task<ActionResult<IEnumerable<TuketimNoktasiDetailDto>>> GetWithDetails()
    {
        var list = await _context.TuketimNoktasis
            .Include(t => t.Ilce)       
            .Include(t => t.Sayaclars)     
            .Include(t => t.Sozlesmelers)
            .Select(t => new TuketimNoktasiDetailDto
            {
                TuketimNoktasiId = t.TuketimNoktasiId,
                TekilKod = t.TekilKod,
                Mahalle = t.Mahalle,
                AcikAdres = t.AcikAdres,
                BaglantiGucuKw = t.BaglantiGucuKw,
                TuketiciGrubu = t.TuketiciGrubu,
                BaglantiDurumu = t.BaglantiDurumu,
                Status = t.Status,


                IlceAdi = t.Ilce != null ? t.Ilce.IlceAdi : "Bilinmiyor",

                AktifSayacSeriNo = t.Sayaclars
                    .Where(s => s.Durum == SayacDurumu.TAKILI)
                    .Select(s => s.SeriNo)
                    .FirstOrDefault() ?? "SAYAÇ YOK",

                AktifAboneId = t.Sozlesmelers
                    .Where(soz => soz.Durum == SozlesmeDurumu.AKTIF)
                    .Select(soz => (long?)soz.AboneId)
                    .FirstOrDefault(),

                AktifSozlesmeNo = t.Sozlesmelers
                    .Where(soz => soz.Durum == SozlesmeDurumu.AKTIF)
                    .Select(soz => soz.SozlesmeNo)
                    .FirstOrDefault() ?? "SÖZLEŞME YOK"
            })
            .ToListAsync();

        return Ok(list);
    }

    // GET BY ID
    [HttpGet("{id}")]
    public async Task<ActionResult<TuketimNoktasi>> GetTuketimNoktasi(long id)
    {
        var nokta = await _context.TuketimNoktasis
            .Include(x => x.Ilce)
            .FirstOrDefaultAsync(x => x.TuketimNoktasiId == id);

        if (nokta == null)
            return NotFound(new { message = "Tüketim noktası bulunamadı." });

        return nokta;
    }

    // POST
    [HttpPost]
    public async Task<ActionResult<TuketimNoktasi>> PostTuketimNoktasi(TuketimNoktasiCreateDto dto)
    {
        if (!await _context.Ilces.AnyAsync(x => x.IlceId == dto.IlceId))
            return BadRequest(new { message = "Böyle bir ilçe bulunamadı." });

        long nextVal = 0;
        string prefix = $"TK-{DateTime.UtcNow:yyyyMM}-";
        
        var connection = _context.Database.GetDbConnection();
        await _context.Database.OpenConnectionAsync();
        try
        {
            using (var command = connection.CreateCommand())
            {
                // Sequence yoksa oluştur ve mevcut max değere set et
                command.CommandText = @"
                    DO $$
                    DECLARE
                        max_val integer;
                    BEGIN
                        IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relkind = 'S' AND relname = 'tuketim_noktasi_seq') THEN
                            CREATE SEQUENCE tuketim_noktasi_seq START 1;
                            
                            SELECT COALESCE(MAX(CAST(NULLIF(SUBSTRING(tekil_kod FROM '[0-9]+$'), '') AS integer)), 0) 
                            INTO max_val 
                            FROM tuketim_noktasi 
                            WHERE tekil_kod LIKE 'TK-%';
                            
                            IF max_val > 0 THEN
                                PERFORM setval('tuketim_noktasi_seq', max_val);
                            END IF;
                        END IF;
                    END
                    $$;";
                await command.ExecuteNonQueryAsync();
                
                // Güvenli ve unique sequence değerini al
                command.CommandText = "SELECT nextval('tuketim_noktasi_seq');";
                var result = await command.ExecuteScalarAsync();
                nextVal = Convert.ToInt64(result);
            }
        }
        finally
        {
            await _context.Database.CloseConnectionAsync();
        }

        string uretilenTekilKod = $"{prefix}{nextVal:D4}";

        var yeniNokta = new TuketimNoktasi
        {
            TekilKod = uretilenTekilKod, 
            IlceId = dto.IlceId,
            Mahalle = dto.Mahalle,
            BinaNo = dto.BinaNo,
            BagimsizBolumNo = dto.BagimsizBolumNo,
            AcikAdres = dto.AcikAdres,
            KoordinatLat = dto.KoordinatLat,
            KoordinatLon = dto.KoordinatLon,
            BaglantiGucuKw = dto.BaglantiGucuKw,
            TuketiciGrubu = dto.TuketiciGrubu,
            BaglantiDurumu = dto.BaglantiDurumu == null ? BaglantiDurumu.PASIF : dto.BaglantiDurumu,
            Status = "AKTIF",
            CreatedAt = DateTime.UtcNow
        };

        _context.TuketimNoktasis.Add(yeniNokta);
        
        _logger.LogInformation("Yeni TuketimNoktasi kaydediliyor. Üretilen TekilKod: {TekilKod}, İlçe: {IlceId}", uretilenTekilKod, dto.IlceId);
        
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            return StatusCode(500, new 
            { 
                message = "An error occurred while saving the entity changes. See the inner exception for details.", 
                innerException = ex.InnerException?.Message ?? ex.Message 
            });
        }

        return CreatedAtAction(nameof(GetTuketimNoktasi), new { id = yeniNokta.TuketimNoktasiId }, yeniNokta);
    }

    // PUT
    [HttpPut("{id}")]
    public async Task<IActionResult> PutTuketimNoktasi(long id, TuketimNoktasi nokta)
    {
        if (id != nokta.TuketimNoktasiId)
            return BadRequest();

        var mevcut = await _context.TuketimNoktasis.FindAsync(id);

        if (mevcut == null)
            return NotFound();

        mevcut.IlceId = nokta.IlceId;
        mevcut.Mahalle = nokta.Mahalle;
        mevcut.BinaNo = nokta.BinaNo;
        mevcut.BagimsizBolumNo = nokta.BagimsizBolumNo;
        mevcut.AcikAdres = nokta.AcikAdres;
        mevcut.KoordinatLat = nokta.KoordinatLat;
        mevcut.KoordinatLon = nokta.KoordinatLon;
        mevcut.BaglantiGucuKw = nokta.BaglantiGucuKw;
        mevcut.TuketiciGrubu = nokta.TuketiciGrubu;
        mevcut.BaglantiDurumu = nokta.BaglantiDurumu;
        mevcut.Status = nokta.Status;
        mevcut.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // SILME
    [HttpDelete("{id}")]
    public async Task<IActionResult> SILMETuketimNoktasi(long id)
    {
        var nokta = await _context.TuketimNoktasis.FindAsync(id);

        if (nokta == null)
            return NotFound();

        bool kullaniliyor = await _context.Sozlesmelers
            .AnyAsync(x => x.TuketimNoktasiId == id);

        if (kullaniliyor)
            return BadRequest(new
            {
                message = "Bu tüketim noktasına ait sözleşme bulunduğu için silinemez."
            });

        _context.TuketimNoktasis.Remove(nokta);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Tüketim noktası başarıyla silindi." });
    }

[HttpPost("generate-fake-tuketim-noktalari")]
public async Task<IActionResult> GenerateFakeTuketimNoktalari()
{
    string prefix = $"TK-{DateTime.UtcNow:yyyyMM}-";
    var sonNokta = await _context.TuketimNoktasis
        .Where(x => x.TekilKod.StartsWith(prefix))
        .OrderByDescending(x => x.TekilKod)
        .FirstOrDefaultAsync();

    int baslangicSira = 1;
    if (sonNokta != null && int.TryParse(sonNokta.TekilKod.Substring(sonNokta.TekilKod.Length - 4), out int sonSira))
    {
        baslangicSira = sonSira + 1;
    }

    var ilceMap = new Dictionary<int, string> 
    {
        { 1, "Melikgazi" },
        { 2, "Kocasinan" },
        { 3, "Talas" }
    };
    var noktaFaker = new Faker<TuketimNoktasi>("tr")

        .RuleFor(t => t.IlceId, f => f.PickRandom(1, 2, 3)) 
        .RuleFor(t => t.Mahalle, f => f.Address.StreetName() + " Mahallesi")
        .RuleFor(t => t.BinaNo, f => f.Address.BuildingNumber())
        .RuleFor(t => t.BagimsizBolumNo, f => f.Random.Number(1, 40).ToString())

        .RuleFor(t => t.AcikAdres, (f, t) => $"{t.Mahalle}, No:{t.BinaNo}, Daire:{t.BagimsizBolumNo} {ilceMap[(int)t.IlceId]}/Kayseri")

        .RuleFor(t => t.BaglantiGucuKw, f => Math.Round(f.Random.Decimal(5.5m, 22.0m), 1))
        .RuleFor(t => t.TuketiciGrubu, f => f.PickRandom(new[] { "MESKEN", "TICARETHANE" }))
        .RuleFor(t => t.BaglantiDurumu, BaglantiDurumu.PASIF)
        .RuleFor(t => t.Status, "AKTIF")
        .RuleFor(t => t.CreatedAt, DateTime.UtcNow);

    var sahteNoktalar = noktaFaker.Generate(50);

    for (int i = 0; i < sahteNoktalar.Count; i++)
    {
        sahteNoktalar[i].TekilKod = $"{prefix}{(baslangicSira + i):D4}";
    }

    _context.TuketimNoktasis.AddRange(sahteNoktalar);
    await _context.SaveChangesAsync();

    return Ok(new 
    { 
        message = "50 sahte tüketim noktası başarıyla oluşturuldu.", 
        eklenenSayi = sahteNoktalar.Count 
    });
}
}

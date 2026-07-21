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

    public TuketimNoktasiController(AppDbContext context)
    {
        _context = context;
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
                    .Where(s => s.Durum == "TAKILI")
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

        // Format: TK-202607-0001
        string prefix = $"TK-{DateTime.UtcNow:yyyyMM}-";
        var sonNokta = await _context.TuketimNoktasis
            .Where(x => x.TekilKod.StartsWith(prefix))
            .OrderByDescending(x => x.TekilKod)
            .FirstOrDefaultAsync();

        int sira = 1;
        if (sonNokta != null && int.TryParse(sonNokta.TekilKod.Substring(sonNokta.TekilKod.Length - 4), out int sonSira))
        {
            sira = sonSira + 1;
        }
        string uretilenTekilKod = $"{prefix}{sira:D4}";

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
            BaglantiDurumu = string.IsNullOrEmpty(dto.BaglantiDurumu) ? "PASIF" : dto.BaglantiDurumu,
            Status = "AKTIF",
            CreatedAt = DateTime.UtcNow
        };

        _context.TuketimNoktasis.Add(yeniNokta);
        await _context.SaveChangesAsync();

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

    // DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTuketimNoktasi(long id)
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
        .RuleFor(t => t.BaglantiDurumu, "PASIF")
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

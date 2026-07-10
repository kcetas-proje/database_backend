using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KcetasAboneApi.Models;
using Microsoft.AspNetCore.Authorization;
using KcetasAboneApi.Models.Dtos;
using Bogus;

namespace KcetasAboneApi.Controllers;

//[Authorize(Roles = "1, 5")]
[Route("api/[controller]")]
[ApiController]
public class IsEmirleriController : ControllerBase
{
    private readonly AppDbContext _context; 

    public IsEmirleriController(AppDbContext context) 
    {
        _context = context;
    }

    // GET: api/IsEmirleri
    [HttpGet]
    public async Task<ActionResult<IEnumerable<IsEmirleri>>> GetIsEmirleri()
    {
        return await _context.IsEmirleris
            .OrderBy(i => i.IsEmriId)
            .ToListAsync();
    }

    // GET: api/IsEmirleri/5
    [HttpGet("{id}")]
    public async Task<ActionResult<IsEmirleri>> GetIsEmri(long id)
    {
        var isEmri = await _context.IsEmirleris.FindAsync(id);

        if (isEmri == null)
        {
            return NotFound(new { message = "İş emri bulunamadı." });
        }

        return isEmri;
    }

    // POST: api/IsEmirleri
    [HttpPost]
    public async Task<ActionResult<IsEmirleri>> PostIsEmri(IsEmriCreateDto dto)
    {
        if (!await _context.TuketimNoktasis.AnyAsync(x => x.TuketimNoktasiId == dto.TuketimNoktasiId))
        {
            return BadRequest(new { message = "Geçersiz tüketim noktası." });
        }

        if (dto.SayacId != null)
        {
            if (!await _context.Sayaclars.AnyAsync(x => x.SayacId == dto.SayacId))
                return BadRequest(new { message = "Sayaç bulunamadı." });
        }

        if (dto.AtananKullaniciId != null)
        {
            if (!await _context.Kullanicilars.AnyAsync(x => x.KullaniciId == dto.AtananKullaniciId))
                return BadRequest(new { message = "Kullanıcı bulunamadı." });
        }

        string prefix = $"IE-{DateTime.UtcNow:yyyyMM}-";

        var sonIsEmri = await _context.IsEmirleris
        .Where(x => x.IsEmriNo.StartsWith(prefix))
        .OrderByDescending(x => x.IsEmriNo)
        .FirstOrDefaultAsync();

        int yeniSira = 1;
        if (sonIsEmri != null)
        {
            string sonSiraStr = sonIsEmri.IsEmriNo.Substring(sonIsEmri.IsEmriNo.Length - 4);
            if (int.TryParse(sonSiraStr, out int sonSiraInt))
            {
                yeniSira = sonSiraInt + 1;
            }
        }

        string yeniIsEmriNo = $"{prefix}{yeniSira:D4}";

        var yeniIsEmri = new IsEmirleri
        {
            IsEmriNo = yeniIsEmriNo,
            TuketimNoktasiId = dto.TuketimNoktasiId,
            SayacId = dto.SayacId,
            AtananKullaniciId = dto.AtananKullaniciId,
            Tip = dto.Tip,
            Oncelik = dto.Oncelik,
            Durum = dto.Durum,
            CreatedAt = DateTime.UtcNow
            
        };

        _context.IsEmirleris.Add(yeniIsEmri);
        await _context.SaveChangesAsync();

        return Ok(new { message = "İş emri oluşturuldu!", data = yeniIsEmri });
    }

    // PUT: api/IsEmirleri/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutIsEmri(long id, IsEmirleri isEmri)
    {
        if (id != isEmri.IsEmriId)
        {
            return BadRequest(new
            {
                message = "İş Emri Id uyuşmuyor."
            });
        }

        var mevcut = await _context.IsEmirleris.FindAsync(id);

        if (mevcut == null)
        {
            return NotFound(new
            {
                message = "İş emri bulunamadı."
            });
        }

        mevcut.TuketimNoktasiId = isEmri.TuketimNoktasiId;
        mevcut.SayacId = isEmri.SayacId;
        mevcut.Tip = isEmri.Tip;
        mevcut.Oncelik = isEmri.Oncelik;
        mevcut.PlanlananTarih = isEmri.PlanlananTarih;
        mevcut.AtananKullaniciId = isEmri.AtananKullaniciId;
        mevcut.Durum = isEmri.Durum;
        mevcut.SahaSonucu = isEmri.SahaSonucu;
        mevcut.Gerekce = isEmri.Gerekce;
        mevcut.MuhurNo = isEmri.MuhurNo;
        mevcut.TutanakNo = isEmri.TutanakNo;
        mevcut.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/IsEmirleri/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteIsEmri(long id)
    {
        var isEmri = await _context.IsEmirleris.FindAsync(id);

        if (isEmri == null)
        {
            return NotFound(new
            {
                message = "İş emri bulunamadı."
            });
        }

        // Endeks okuma kontrolü
        bool kullaniliyor = await _context.EndeksOkumas
            .AnyAsync(x => x.IsEmriId == id);

        if (kullaniliyor)
        {
            return BadRequest(new
            {
                message = "Bu iş emrine bağlı endeks okuma bulunduğu için silinemez."
            });
        }

        _context.IsEmirleris.Remove(isEmri);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("tamamla/{isEmriId}/sayac/{sayacId}")]
    public async Task<IActionResult> IsEmriTamamla(long isEmriId, long sayacId)
    {
        var isEmri = await _context.IsEmirleris.FindAsync(isEmriId);
        if (isEmri == null || isEmri.Durum != "ACIK")
            return BadRequest(new { message = "Böyle bir açık iş emri yok." });

        var sayac = await _context.Sayaclars.FindAsync(sayacId);
        if (sayac == null || sayac.Durum != "DEPODA")
            return BadRequest(new { message = "Bu sayaç depoda değil." });

        isEmri.SayacId = sayac.SayacId; 
        isEmri.Durum = "TAMAMLANDI";
        
        isEmri.UpdatedAt = DateTime.UtcNow; 

        sayac.TuketimNoktasiId = isEmri.TuketimNoktasiId;
        sayac.Durum = "TAKILI";
        sayac.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new 
        { 
            message = $"İş emri {isEmri.IsEmriNo} başarıyla tamamlandı ve sayaç {sayac.SeriNo} takıldı!", 
        });
    }

    [HttpPost("toplu-yeni-baglanti-onayla")]
    public async Task<IActionResult> TopluYeniBaglantiOnayla()
    {

        var acikIsEmirleri = await _context.IsEmirleris
            .Where(i => i.Tip == "YENI_BAGLANTI" && i.Durum == "ACIK")
            .ToListAsync();

        var depodakiSayaclar = await _context.Sayaclars
            .Where(s => s.Durum == "DEPODA")
            .ToListAsync();

        int islemKapasitesi = Math.Min(acikIsEmirleri.Count, depodakiSayaclar.Count);

        if (islemKapasitesi == 0)
            return BadRequest(new { message = "Sahada açık iş emri yok ya da depoda sayaç kalmamış!" });

        for (int i = 0; i < islemKapasitesi; i++)
        {
            var isEmri = acikIsEmirleri[i];
            var sayac = depodakiSayaclar[i];

            isEmri.SayacId = sayac.SayacId; 
            isEmri.Durum = "TAMAMLANDI";

            sayac.TuketimNoktasiId = isEmri.TuketimNoktasiId;
            sayac.Durum = "TAKILI";
        }

        await _context.SaveChangesAsync();

        return Ok(new 
        { 
            message = $"{islemKapasitesi} adet YENI_BAGLANTI iş emri başarıyla tamamlandı ve sayaçlar mekanlara takıldı!",
            onaylananSayi = islemKapasitesi
        });
    }

    [HttpPost("generate-random-is-emirleri")]
    public async Task<IActionResult> GenerateRandomIsEmirleri()
    {

        var mekanlar = await _context.TuketimNoktasis.ToListAsync();
        if (!mekanlar.Any())
            return BadRequest(new { message = "Tüketim noktaları bulunamadı." });

        string isEmriPrefix = $"IE-{DateTime.UtcNow:yyyyMM}-";

        int isEmriSira = 1;
        var sonIsEmri = await _context.IsEmirleris
            .Where(x => x.IsEmriNo.StartsWith(isEmriPrefix))
            .OrderByDescending(x => x.IsEmriNo)
            .FirstOrDefaultAsync();

        if (sonIsEmri != null && int.TryParse(sonIsEmri.IsEmriNo.Substring(sonIsEmri.IsEmriNo.Length - 4), out int iSira))
            isEmriSira = iSira + 1;

        var isEmriTipleri = new[] { "BAGLAMA","DEGISTIRME","SOKME","KESME","ACMA","ENDEKS_OKUMA","SAYAC_ARIZA","MUHURLEME","KESIF_INCELEME","YENI_BAGLANTI"};
        var oncelikler = new[] { "DUSUK", "NORMAL", "YUKSEK", "ACIL" };

        var isEmriFaker = new Faker<IsEmirleri>("tr")
            .RuleFor(i => i.TuketimNoktasiId, f => f.PickRandom(mekanlar).TuketimNoktasiId)
            .RuleFor(i => i.Tip, f => f.PickRandom(isEmriTipleri))
            
            // Arıza veya Kaçak İhbarı varsa aciliyet artar fr fr
            .RuleFor(i => i.Oncelik, (f, i) => i.Tip == "ARIZA" ? "ACIL" : f.PickRandom(oncelikler))
            
            .RuleFor(i => i.Durum, "ACIK")
            .RuleFor(i => i.CreatedAt, f => f.Date.Recent(7).ToUniversalTime());

        var sahteIsEmirleri = isEmriFaker.Generate(30);

        for (int j = 0; j < sahteIsEmirleri.Count; j++)
        {
            sahteIsEmirleri[j].IsEmriNo = $"{isEmriPrefix}{(isEmriSira + j):D4}";
        }

        _context.IsEmirleris.AddRange(sahteIsEmirleri);
        await _context.SaveChangesAsync();

        return Ok(new 
        { 
            message = "30 sahte iş emri başarıyla oluşturuldu.", 
            eklenenSayi = sahteIsEmirleri.Count 
        });
    }
}

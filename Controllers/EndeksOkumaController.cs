using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KcetasAboneApi.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using KcetasAboneApi.Models.Dtos;
using Bogus;

namespace KcetasAboneApi.Controllers;

//[Authorize(Roles = "1, 4")]
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
    [HttpGet("GetWithDetails")]
    public async Task<ActionResult<IEnumerable<EndeksOkumaDetailDto>>> GetWithDetails()
    {
        var list = await _context.EndeksOkumas
            .Include(e => e.Sayac)
                .ThenInclude(s => s.TuketimNoktasi)
            .Include(e => e.Sozlesme) 
            .Select(e => new EndeksOkumaDetailDto
            {
                OkumaId = e.OkumaId, 
                SayacId = e.SayacId,
                IsEmriId = e.IsEmriId,
                SozlesmeId = e.SozlesmeId,
                OkumaTipi = e.OkumaTipi,
                OkumaKaynagi = e.OkumaKaynagi,
                OncekiEndeks = e.OncekiEndeks,
                YeniEndeks = e.YeniEndeks,
                Donem = e.Donem,
                OkumaZamani = e.OkumaZamani,
                KullaniciId = e.KullaniciId,

                AboneId = e.Sozlesme != null ? e.Sozlesme.AboneId : null, 

                SeriNo = e.Sayac != null ? e.Sayac.SeriNo : "-",
                MarkaModel = e.Sayac != null ? $"{e.Sayac.Marka} {e.Sayac.Model}" : "-",
                Mahalle = (e.Sayac != null && e.Sayac.TuketimNoktasi != null) ? e.Sayac.TuketimNoktasi.Mahalle : "-",
                AcikAdres = (e.Sayac != null && e.Sayac.TuketimNoktasi != null) ? e.Sayac.TuketimNoktasi.AcikAdres : "Adres Tanımsız",
                TuketiciGrubu = (e.Sayac != null && e.Sayac.TuketimNoktasi != null) ? e.Sayac.TuketimNoktasi.TuketiciGrubu : "MESKEN"
            })
            .ToListAsync();

        return Ok(list);
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
            
            OkumaTipi = dto.OkumaTipi ?? OkumaTipi.RUTIN_DONEM,
            OkumaKaynagi = dto.OkumaKaynagi ?? OkumaKaynagi.MANUEL,
            Donem = string.IsNullOrEmpty(dto.Donem) ? DateTime.Now.ToString("yyyy-MM") : dto.Donem,
            OkumaZamani = dto.OkumaZamani ?? DateTime.UtcNow,
            KullaniciId = dto.KullaniciId,

            DogrulamaDurumu = DogrulamaDurumu.DOGRULAMA_BEKLIYOR, 
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
    endeks.OkumaTipi = System.Enum.Parse<OkumaTipi>(dto.OkumaTipi.ToString());

    await _context.SaveChangesAsync();
    return NoContent();
}

    [HttpDelete("{id}")]
public async Task<IActionResult> Delete(long id)
{
    var endeks = await _context.EndeksOkumas.FindAsync(id);
    if (endeks == null) return NotFound();

    endeks.Status = BaglantiDurumu.PASIF.ToString();

    await _context.SaveChangesAsync();
    return NoContent(); 
}

[HttpPost("generate-fake-endeksler")]
public async Task<IActionResult> GenerateFakeEndeksler()
{
    var sozlesmeler = await _context.Sozlesmelers.ToListAsync();
    var takiliSayaclar = await _context.Sayaclars
        .Where(s => s.Durum == SayacDurumu.TAKILI && s.TuketimNoktasiId != null)
        .ToListAsync();

    if (!sozlesmeler.Any() || !takiliSayaclar.Any())
        return BadRequest(new { message = "Sözleşmeler veya takılı sayaçlar bulunamadı." });

    var sahteEndeksler = new List<EndeksOkuma>();
    var f = new Faker("tr"); 

    foreach (var sozlesme in sozlesmeler)
    {
        var sayac = takiliSayaclar.FirstOrDefault(s => s.TuketimNoktasiId == sozlesme.TuketimNoktasiId);
        if (sayac == null) continue; 

        var ilgiliIsEmri = await _context.IsEmirleris
            .Where(ie => ie.TuketimNoktasiId == sozlesme.TuketimNoktasiId)
            .OrderByDescending(ie => ie.CreatedAt)
            .FirstOrDefaultAsync();

        var sonOkuma = await _context.EndeksOkumas
            .Where(e => e.SayacId == sayac.SayacId)
            .OrderByDescending(e => e.CreatedAt)
            .FirstOrDefaultAsync();

        var yeniEndeks = new EndeksOkuma
        {
            SayacId = sayac.SayacId,
            SozlesmeId = sozlesme.SozlesmeId,
            IsEmriId = ilgiliIsEmri?.IsEmriId,
            OkumaKaynagi = OkumaKaynagi.MANUEL,
            Donem = $"{DateTime.UtcNow:yyyy/MM}",
            OkumaZamani = DateTime.UtcNow,
            KullaniciId = _context.MevcutKullaniciId,
            DogrulamaDurumu = DogrulamaDurumu.ONAYLANDI,
            AnomaliMi = false,
            Status = "AKTIF",
            CreatedAt = DateTime.UtcNow
        };

        if (sonOkuma == null)
        {

            yeniEndeks.OkumaTipi = OkumaTipi.ILK_OKUMA;
            yeniEndeks.OncekiEndeks = 0m;
            yeniEndeks.YeniEndeks = Math.Round(f.Random.Decimal(50m, 150m), 3);
        }
        else
        {

            yeniEndeks.OkumaTipi = OkumaTipi.RUTIN_DONEM;
            yeniEndeks.OncekiEndeks = sonOkuma.YeniEndeks;
            yeniEndeks.YeniEndeks = sonOkuma.YeniEndeks + Math.Round(f.Random.Decimal(100m, 400m), 3);
        }

        sahteEndeksler.Add(yeniEndeks);
    }

    _context.EndeksOkumas.AddRange(sahteEndeksler);
    await _context.SaveChangesAsync();

    return Ok(new 
    { 
        message = $"{sahteEndeksler.Count} adet sahte endeks okuma başarıyla oluşturuldu.",
        eklenenSayi = sahteEndeksler.Count
    });
}
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KcetasAboneApi.Models; 

namespace KcetasAboneApi.Controllers;

[ApiController]
[Route("api/[controller]")]

public class SozlesmelerController : ControllerBase
{
    private readonly AppDbContext _context; 

    public SozlesmelerController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateSozlesme([FromBody] SozlesmeCreateDto dto)
    {
        var mevcutAktif = await _context.Sozlesmelers
            .FirstOrDefaultAsync(s => s.TuketimNoktasiId == dto.TuketimNoktasiId && s.Statu == "AKTIF");

        if (mevcutAktif != null)
            return BadRequest("Bu tesisatta zaten aktif bir sözleşme var, önce onu feshetmelisin!");

        var yeniSozlesme = new Sozlesmeler
        {
            TuketimNoktasiId = dto.TuketimNoktasiId,
            SozlesmeNo = dto.SozlesmeNo,
            Ad = dto.Ad,
            Soyad = dto.Soyad,
            Tckn = dto.Tckn,
            SozlesmeTipi = dto.SozlesmeTipi,
            BaslangicTarihi = DateOnly.FromDateTime(dto.BaslangicTarihi),
            TarifeGrubu = dto.TarifeGrubu,
            GuvenceBedeli = dto.GuvenceBedeli,
            Statu = "AKTIF" 
        };

        _context.Sozlesmelers.Add(yeniSozlesme);
        await _context.SaveChangesAsync();

        return Ok(yeniSozlesme);
    }
}
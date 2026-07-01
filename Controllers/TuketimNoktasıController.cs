using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KcetasAboneApi.Models;

namespace KcetasAboneApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TuketimNoktasiController : ControllerBase
{
    private readonly AppDbContext _context;

    public TuketimNoktasiController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetTesisatlar()
    {
        // Artık 'abone' tablosu yok, doğrudan tüketim noktası ve sözleşmeleri çekiyoruz
        var tesisatlar = await _context.TuketimNoktasis
            .Include(t => t.Sozlesmelers) // Sözleşmeleri dahil et
            .Include(t => t.Ilce)         // İlçe bilgisini getir
            .ToListAsync();
            
        return Ok(tesisatlar);
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KcetasAboneApi.Models;
using Microsoft.AspNetCore.Authorization;

namespace KcetasAboneApi.Controllers;

//[Authorize(Roles = "1, 2")]
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
        var tesisatlar = await _context.TuketimNoktasis
            .Include(t => t.Sozlesmelers) 
            .Include(t => t.Ilce)         
            .ToListAsync();
            
        return Ok(tesisatlar);
    }
}
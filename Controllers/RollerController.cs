using KcetasAboneApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

//[Authorize(Roles = "1")]
[ApiController]
[Route("api/[controller]")]
public class RollerController : ControllerBase
{
    private readonly AppDbContext _context;

    public RollerController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Sistemdeki yetki rollerini (Admin, Şef, Saha Elemanı vb.) listeler.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var roller = await _context.Rollers.ToListAsync();
        return Ok(roller);
    }
}
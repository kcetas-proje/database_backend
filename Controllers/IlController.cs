using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KcetasAboneApi.Models;

[Route("api/[controller]")]
[ApiController]
public class IlController : ControllerBase
{
    private readonly AppDbContext _context;

    public IlController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Sistemdeki tüm illeri listeler.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Il>>> GetIller()
    {
        return await _context.Ils.ToListAsync();
    }
}
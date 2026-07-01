using KcetasAboneApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class RollerController : ControllerBase
{
    private readonly AppDbContext _context;

    public RollerController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var roller = await _context.Rollers.ToListAsync();
        return Ok(roller);
    }
}
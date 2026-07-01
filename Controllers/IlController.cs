using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KcetasAboneApi.Data;
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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Il>>> GetIller()
    {
        return await _context.Iller.ToListAsync();
    }
}
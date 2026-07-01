using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KcetasAboneApi.Models;

namespace KcetasAboneApi.Controllers;

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
    public async Task<ActionResult<EndeksOkuma>> Post(EndeksOkuma model)
    {
        _context.EndeksOkumas.Add(model);

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(Get),
            new { id = model.OkumaId }, model);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(long id, EndeksOkuma model)
    {
        if (id != model.OkumaId)
            return BadRequest();

        _context.Entry(model).State = EntityState.Modified;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var okuma = await _context.EndeksOkumas.FindAsync(id);

        if (okuma == null)
            return NotFound();

        _context.EndeksOkumas.Remove(okuma);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}

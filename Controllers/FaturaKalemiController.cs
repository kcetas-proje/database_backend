using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KcetasAboneApi.Models;

namespace KcetasAboneApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FaturaKalemiController : ControllerBase
{
    private readonly AppDbContext _context;

    public FaturaKalemiController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FaturaKalemi>>> Get()
    {
        return await _context.FaturaKalemis
            .Include(x => x.Fatura)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FaturaKalemi>> Get(long id)
    {
        var kalem = await _context.FaturaKalemis
            .Include(x => x.Fatura)
            .FirstOrDefaultAsync(x => x.FaturaKalemId == id);

        if (kalem == null)
            return NotFound();

        return kalem;
    }

    [HttpPost]
    public async Task<ActionResult<FaturaKalemi>> Post(FaturaKalemi model)
    {
        _context.FaturaKalemis.Add(model);

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(Get),
            new { id = model.FaturaKalemId }, model);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(long id, FaturaKalemi model)
    {
        if (id != model.FaturaKalemId)
            return BadRequest();

        _context.Entry(model).State = EntityState.Modified;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var kalem = await _context.FaturaKalemis.FindAsync(id);

        if (kalem == null)
            return NotFound();

        _context.FaturaKalemis.Remove(kalem);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}

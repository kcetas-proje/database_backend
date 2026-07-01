using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KcetasAboneApi.Models;

namespace KcetasAboneApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EntegrasyonOutboxController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EntegrasyonOutboxController(AppDbContext context)
        {
            _context = context;
        }

       
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EntegrasyonOutbox>>> GetEntegrasyonOutboxes()
        {
            return await _context.EntegrasyonOutboxes
                .Include(e => e.Fatura)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();
        }

        
        [HttpGet("{id}")]
        public async Task<ActionResult<EntegrasyonOutbox>> GetEntegrasyonOutbox(long id)
        {
            var outbox = await _context.EntegrasyonOutboxes
                .Include(e => e.Fatura)
                .FirstOrDefaultAsync(e => e.OutboxId == id);

            if (outbox == null)
            {
                return NotFound();
            }

            return outbox;
        }

       
        [HttpPost]
        public async Task<ActionResult<EntegrasyonOutbox>> PostEntegrasyonOutbox(EntegrasyonOutbox outbox)
        {
            outbox.CreatedAt = DateTime.UtcNow;

            _context.EntegrasyonOutboxes.Add(outbox);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEntegrasyonOutbox),
                new { id = outbox.OutboxId }, outbox);
        }

        
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEntegrasyonOutbox(long id, EntegrasyonOutbox outbox)
        {
            if (id != outbox.OutboxId)
            {
                return BadRequest();
            }

            _context.Entry(outbox).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.EntegrasyonOutboxes.Any(e => e.OutboxId == id))
                {
                    return NotFound();
                }

                throw;
            }

            return NoContent();
        }

        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEntegrasyonOutbox(long id)
        {
            var outbox = await _context.EntegrasyonOutboxes.FindAsync(id);

            if (outbox == null)
            {
                return NotFound();
            }

            _context.EntegrasyonOutboxes.Remove(outbox);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

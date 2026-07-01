using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KcetasAboneApi.Models;

namespace KcetasAboneApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditLogController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuditLogController(AppDbContext context)
        {
            _context = context;
        }

        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuditLog>>> GetAuditLogs()
        {
            return await _context.AuditLogs
                .Include(a => a.Kullanici)
                .OrderByDescending(a => a.IslemZamani)
                .ToListAsync();
        }

        
        [HttpGet("{id}")]
        public async Task<ActionResult<AuditLog>> GetAuditLog(long id)
        {
            var auditLog = await _context.AuditLogs
                .Include(a => a.Kullanici)
                .FirstOrDefaultAsync(a => a.AuditId == id);

            if (auditLog == null)
            {
                return NotFound();
            }

            return auditLog;
        }

        
        [HttpPost]
        public async Task<ActionResult<AuditLog>> PostAuditLog(AuditLog auditLog)
        {
            auditLog.IslemZamani = DateTime.UtcNow;

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAuditLog),
                new { id = auditLog.AuditId }, auditLog);
        }

        
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAuditLog(long id, AuditLog auditLog)
        {
            if (id != auditLog.AuditId)
            {
                return BadRequest();
            }

            _context.Entry(auditLog).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.AuditLogs.Any(e => e.AuditId == id))
                {
                    return NotFound();
                }

                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAuditLog(long id)
        {
            var auditLog = await _context.AuditLogs.FindAsync(id);

            if (auditLog == null)
            {
                return NotFound();
            }

            _context.AuditLogs.Remove(auditLog);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

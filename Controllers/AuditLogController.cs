using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KcetasAboneApi.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace KcetasAboneApi.Controllers
{
    [Authorize(Roles = "1, 7")]
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
        [HttpPost]
        public async Task<ActionResult<AuditLog>> PostAuditLog([FromBody] AuditLogCreateDto dto)
        {
            var auditLog = new AuditLog
            {
                VarlikTipi = dto.VarlikTipi, 
                VarlikId = dto.VarlikId,
                IslemTipi = dto.IslemTipi,
                EskiDeger = dto.EskiDeger,
                YeniDeger = dto.YeniDeger,
                KullaniciId = dto.KullaniciId,
                IslemGerekcesi = dto.IslemGerekcesi,
                IslemZamani = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAuditLog), new { id = auditLog.AuditId }, auditLog);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuditLog>> GetAuditLog(long id)
        {
            var auditLog = await _context.AuditLogs
                .Include(a => a.Kullanici)
                .FirstOrDefaultAsync(a => a.AuditId == id);

            if (auditLog == null) return NotFound();
            return auditLog;
        
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
    }
}

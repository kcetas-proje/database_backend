using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KcetasAboneApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace KcetasAboneApi.Controllers
{   
    //[Authorize(Roles = "1, 7")]
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
        public async Task<IActionResult> GetEntegrasyonOutboxes(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 50,
            [FromQuery] OutboxDurumu? durum = null,
            [FromQuery] HedefSistem? hedefSistem = null,
            [FromQuery] string? faturaNo = null,
            [FromQuery] long? kayitNo = null,
            [FromQuery] string? baslangic = null,
            [FromQuery] string? bitis = null,
            [FromQuery] string? sonDenemeTarihiStart = null,
            [FromQuery] string? sonDenemeTarihiEnd = null)
        {
            var query = _context.EntegrasyonOutboxes.AsNoTracking().AsQueryable();

            if (kayitNo.HasValue)
            {
                query = query.Where(x => x.OutboxId == kayitNo.Value);
            }

            if (durum.HasValue)
            {
                query = query.Where(x => x.Durum == durum.Value);
            }

            if (hedefSistem.HasValue)
            {
                query = query.Where(x => x.HedefSistem == hedefSistem.Value);
            }

            if (!string.IsNullOrEmpty(faturaNo))
            {
                query = query.Where(x => x.Fatura != null && x.Fatura.FaturaNo == faturaNo);
            }

            if (!string.IsNullOrWhiteSpace(baslangic) && DateTime.TryParse(baslangic, out var baslangicDate))
            {
                var utcDate = DateTime.SpecifyKind(baslangicDate, DateTimeKind.Utc);
                query = query.Where(x => x.CreatedAt >= utcDate);
            }
            
            if (!string.IsNullOrWhiteSpace(bitis) && DateTime.TryParse(bitis, out var bitisDate))
            {
                var utcDate = DateTime.SpecifyKind(bitisDate, DateTimeKind.Utc);
                query = query.Where(x => x.CreatedAt <= utcDate);
            }

            if (!string.IsNullOrWhiteSpace(sonDenemeTarihiStart) && DateTime.TryParse(sonDenemeTarihiStart, out var sonDenemeStart))
            {
                var utcDate = DateTime.SpecifyKind(sonDenemeStart, DateTimeKind.Utc);
                query = query.Where(x => x.SonDenemeTarihi >= utcDate);
            }
            
            if (!string.IsNullOrWhiteSpace(sonDenemeTarihiEnd) && DateTime.TryParse(sonDenemeTarihiEnd, out var sonDenemeEnd))
            {
                var utcDate = DateTime.SpecifyKind(sonDenemeEnd, DateTimeKind.Utc);
                query = query.Where(x => x.SonDenemeTarihi <= utcDate);
            }

            var total = await query.CountAsync();

            var data = await query
                .Include(e => e.Fatura)
                .OrderByDescending(e => e.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new 
            {
                TotalCount = total,
                Page = page,
                PageSize = pageSize,
                Data = data
            });
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
        public async Task<ActionResult<EntegrasyonOutbox>> PostEntegrasyonOutbox([FromBody] EntegrasyonOutboxCreateDto dto)
        {
            var yeniMesaj = new EntegrasyonOutbox
            {
                FaturaId = dto.FaturaId,
                HedefSistem = dto.HedefSistem,
                Payload = dto.Payload,

                IdempotencyKey = Guid.NewGuid().ToString(),
                CorrelationId = Guid.NewGuid().ToString(),
                
                Durum = OutboxDurumu.BEKLIYOR,
                RetryCount = 0,
                CreatedAt = DateTime.UtcNow
            };

            _context.EntegrasyonOutboxes.Add(yeniMesaj);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEntegrasyonOutbox), new { id = yeniMesaj.OutboxId }, yeniMesaj);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] EntegrasyonOutboxUpdateDto dto)
        {
            var log = await _context.EntegrasyonOutboxes.FindAsync(id);
            if (log == null) return NotFound();

            log.Durum = System.Enum.Parse<OutboxDurumu>(dto.Durum);
            log.RetryCount = dto.RetryCount;
            log.HataMesaji = dto.HataMesaji;
            log.SonDenemeTarihi = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> SILMEEntegrasyonOutbox(long id)
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
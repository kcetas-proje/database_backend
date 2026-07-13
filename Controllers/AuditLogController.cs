using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KcetasAboneApi.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace KcetasAboneApi.Controllers;

//[Authorize(Roles = "1, 7")] 
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
    public async Task<IActionResult> GetAuditLogs([FromQuery] int sayfa = 1, [FromQuery] int limit = 50)
    {
        var loglar = await _context.AuditLogs
            .Include(a => a.Kullanici)
            .OrderByDescending(a => a.IslemZamani)
            .Skip((sayfa - 1) * limit)
            .Take(limit) 
            .Select(a => new 
            {
                a.AuditId,
                a.VarlikTipi,
                a.VarlikId,
                a.IslemTipi,
                a.IslemGerekcesi,
                a.IslemZamani,
                KullaniciAdi = a.Kullanici != null ? a.Kullanici.AdSoyad : "Sistem Otomasyonu",
                a.EskiDeger,
                a.YeniDeger
            })
            .ToListAsync();

        return Ok(new 
        {
            Sayfa = sayfa,
            Limit = limit,
            Data = loglar
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAuditLog(long id)
    {
        var auditLog = await _context.AuditLogs
            .Include(a => a.Kullanici)
            .Select(a => new 
            {
                a.AuditId,
                a.VarlikTipi,
                a.VarlikId,
                a.IslemTipi,
                a.IslemGerekcesi,
                a.IslemZamani,
                KullaniciAdi = a.Kullanici != null ? a.Kullanici.AdSoyad : "Sistem Otomasyonu",
                a.EskiDeger,
                a.YeniDeger
            })
            .FirstOrDefaultAsync(a => a.AuditId == id);

        if (auditLog == null) return NotFound(new { message = "Böyle bir log bulunamadı." });
        
        return Ok(auditLog);
    }

}
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
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 50,
        [FromQuery] string? q = null,
        [FromQuery] string? kullanici = null,
        [FromQuery] long? kayitId = null,
        [FromQuery] string? islemTipi = null,
        [FromQuery] string? tarih = null)
    {
        var query = _context.AuditLogs
            .Include(a => a.Kullanici)
            .AsNoTracking()
            .AsQueryable();

        // 1. kayitId
        if (kayitId.HasValue)
        {
            query = query.Where(a => a.VarlikId == kayitId.Value);
        }

        // 2. islemTipi
        if (!string.IsNullOrWhiteSpace(islemTipi) && Enum.TryParse<IslemTipi>(islemTipi, true, out var parsedIslemTipi))
        {
            query = query.Where(a => a.IslemTipi == parsedIslemTipi);
        }

        // 3. tarih
        if (!string.IsNullOrWhiteSpace(tarih) && DateTime.TryParse(tarih, out var parsedDate))
        {
            var baslangic = DateTime.SpecifyKind(parsedDate.Date, DateTimeKind.Utc);
            var bitis = baslangic.AddDays(1).AddTicks(-1);
            query = query.Where(a => a.IslemZamani >= baslangic && a.IslemZamani <= bitis);
        }

        // 4. q (VarlikTipi, IslemGerekcesi)
        if (!string.IsNullOrWhiteSpace(q))
        {
            var qLower = q.ToLower();
            query = query.Where(a => 
                a.VarlikTipi.ToLower().Contains(qLower) || 
                (a.IslemGerekcesi != null && a.IslemGerekcesi.ToLower().Contains(qLower))
            );
        }

        // 5. kullanici (AdSoyad, KullaniciAdi)
        if (!string.IsNullOrWhiteSpace(kullanici))
        {
            var kullaniciLower = kullanici.ToLower();
            query = query.Where(a => 
                a.Kullanici != null && 
                (a.Kullanici.AdSoyad.ToLower().Contains(kullaniciLower) || 
                 a.Kullanici.KullaniciAdi.ToLower().Contains(kullaniciLower))
            );
        }

        var total = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(total / (double)pageSize);

        var loglar = await query
            .OrderByDescending(a => a.IslemZamani)
            .Skip((page - 1) * pageSize)
            .Take(pageSize) 
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
            data = loglar,
            currentPage = page,
            pageSize = pageSize,
            totalPages = totalPages,
            totalCount = total
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
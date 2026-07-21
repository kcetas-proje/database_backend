using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using KcetasAboneApi.Models;
using Microsoft.AspNetCore.SignalR; 
using KcetasAboneApi.Hubs;          

[Route("api/[controller]")]
[ApiController]
public class BildirimlerController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IHubContext<BildirimHub> _hubContext;

    public BildirimlerController(AppDbContext context, IHubContext<BildirimHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    // 1. ŞEFİN GÖREVİ: Bildirim Gönderme / Kaydetme Fonksiyonu
    [HttpPost("Send")]
    public async Task<IActionResult> SendNotification([FromQuery] int userId, [FromQuery] string baslik, [FromQuery] string icerik)
    {
        var yeniBildirim = new Bildirim
        {
            KullaniciId = userId,
            Baslik = baslik,
            Icerik = icerik,
            OkunduMu = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Bildirimler.Add(yeniBildirim);
        await _context.SaveChangesAsync();


        await _hubContext.Clients.User(userId.ToString()).SendAsync("YeniBildirimGeldi", baslik, icerik);

        return Ok(new { mesaj = "Bildirim başarıyla kaydedildi ve gönderildi.", data = yeniBildirim });
    }

    [HttpGet("MyNotifications/{userId}")]
    public async Task<IActionResult> GetMyNotifications(int userId)
    {
        var bildirimler = await _context.Bildirimler
            .AsNoTracking()
            .Where(b => b.KullaniciId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        return Ok(bildirimler);
    }

    [HttpPut("OkunduIsaretle/{bildirimId}")]
    public async Task<IActionResult> OkunduIsaretle(int bildirimId)
    {
        // Bildirimi DB'den çek
        var bildirim = await _context.Bildirimler.FindAsync(bildirimId);
        
        if (bildirim == null)
        {
            return NotFound(new { message = "Böyle bir bildirim bulunamadı." });
        }

        if (bildirim.OkunduMu)
        {
            return BadRequest(new { message = "Bu bildirim zaten okunmuş." });
        }

        bildirim.OkunduMu = true;
        
        await _context.SaveChangesAsync();

        return Ok(new { message = "Bildirim başarıyla okundu olarak işaretlendi." });
    }
}
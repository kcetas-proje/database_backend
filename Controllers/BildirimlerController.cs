using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using KcetasAboneApi.Models;

[Route("api/[controller]")]
[ApiController]
public class BildirimlerController : ControllerBase
{
    private readonly AppDbContext _context;

    public BildirimlerController(AppDbContext context)
    {
        _context = context;
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

        // Veritabanına kaydet (Loglama)
        _context.Bildirimlers.Add(yeniBildirim);
        await _context.SaveChangesAsync();

        // NOT: İleride Firebase (FCM) veya OneSignal eklerseniz, 
        // telefona anlık titreme/bildirim gönderme kodunu tam buraya yazacaksın!

        return Ok(new { mesaj = "Bildirim başarıyla kaydedildi ve gönderildi!", data = yeniBildirim });
    }

    [HttpGet("MyNotifications/{userId}")]
    public async Task<IActionResult> GetMyNotifications(int userId)
    {
        var bildirimler = await _context.Bildirimlers
            .AsNoTracking()
            .Where(b => b.KullaniciId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        return Ok(bildirimler);
    }
}
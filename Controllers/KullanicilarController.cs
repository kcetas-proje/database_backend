using KcetasAboneApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KcetasAboneApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KullaniciController : ControllerBase
    {
        private readonly AppDbContext _context;

        public KullaniciController(AppDbContext context)
        {
            _context = context;
        }

        
        [HttpGet]
        public async Task<IActionResult> GetKullanicilar()
        {
            var kullanicilar = await _context.Kullanicilars
                .Where(k => k.Durum == "AKTIF")
                .ToListAsync();

            if (!kullanicilar.Any())
            {
                return NotFound("Sistemde aktif kullanıcı bulunamadı.");
            }

            return Ok(kullanicilar);
        }

        
        [HttpPost]
        public async Task<IActionResult> YeniKullaniciEkle([FromBody] Kullanicilar yeniKullanici)
        {
            yeniKullanici.Durum = "AKTIF";
            yeniKullanici.CreatedAt = DateTime.UtcNow;

            _context.Kullanicilars.Add(yeniKullanici);
            await _context.SaveChangesAsync();

            return Ok(yeniKullanici);
        }

        
        [HttpPut("{id}")]
        public async Task<IActionResult> KullaniciGuncelle(long id, [FromBody] Kullanicilar guncelKullanici)
        {
            var dbKullanici = await _context.Kullanicilars.FindAsync(id);

            if (dbKullanici == null)
            {
                return NotFound("Böyle bir kullanıcı bulunamadı.");
            }

            dbKullanici.AdSoyad = guncelKullanici.AdSoyad;
            dbKullanici.KullaniciAdi = guncelKullanici.KullaniciAdi;
            dbKullanici.EPosta = guncelKullanici.EPosta;
            dbKullanici.SifreHash = guncelKullanici.SifreHash;
            dbKullanici.RolId = guncelKullanici.RolId;
            dbKullanici.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(dbKullanici);
        }

       
        [HttpDelete("{id}")]
        public async Task<IActionResult> KullaniciSil(long id)
        {
            var dbKullanici = await _context.Kullanicilars.FindAsync(id);

            if (dbKullanici == null)
            {
                return NotFound("Böyle bir kullanıcı bulunamadı.");
            }

            dbKullanici.Durum = "PASIF";
            dbKullanici.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mesaj = $"{dbKullanici.KullaniciAdi} kullanıcısı başarıyla pasif duruma alındı."
            });
        }
    }
}


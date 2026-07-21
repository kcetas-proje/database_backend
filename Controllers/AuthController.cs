using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using KcetasAboneApi.Models; 
using KcetasAboneApi.Services;

namespace KcetasAboneApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;

        public AuthController(AppDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            // 1. Kullanıcıyı ve bağlı olduğu Rol tablosunu veritabanından çekiyoruz (Include = SQL JOIN işlemi)
            var user = await _context.Kullanicilars
                .Include(k => k.Rol)
                .FirstOrDefaultAsync(k => k.KullaniciAdi == dto.KullaniciAdi && k.SifreHash == dto.Sifre); 
                
                // ÖNEMLİ NOT: Modelde alan adı 'SifreHash' geçiyor ancak MVP test aşamasında olduğumuz için 
                // şifreyi düz metin (plain text) olarak karşılaştırıyoruz. Canlıya çıkarken buraya SHA256 
                // gibi bir şifreleme algoritması eklenmelidir.

            // 2. Kullanıcı veritabanında yoksa veya şifre hatalıysa yetkisiz giriş hatası dönülür
            if (user == null)
            {
                return Unauthorized("Kullanıcı adı veya şifre hatalı.");
            }

            // 3. Kullanıcının hesabı pasife alınmışsa sisteme girişi engellenir
            if (user.Durum != KullaniciDurumu.AKTIF)
            {
                return BadRequest("Hesabınız aktif durumda değil. Lütfen sistem yöneticisiyle görüşün.");
            }

            var token = _jwtService.GenerateToken(user.KullaniciAdi, user.RolId);

            // 5. Token ve kullanıcı bilgileri ön yüze dönülür
            return Ok(new 
            { 
                Token = token, 
                Mesaj = $"Giriş başarılı. Hoş geldiniz, {user.AdSoyad}.",
                Yetki = user.Rol.RolAdi
            });
        }
    }

    public class LoginDto
    {
        public string KullaniciAdi { get; set; }
        public string Sifre { get; set; }
    }
}
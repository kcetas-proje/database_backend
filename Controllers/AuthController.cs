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

        /// <summary>
        /// Kullanıcı girişi (login) işlemini yapar ve başarılı olursa JWT yetkilendirme token'ı döndürür.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {

            var user = await _context.Kullanicilars
                .Include(k => k.Rol)
                .FirstOrDefaultAsync(k => k.KullaniciAdi == dto.KullaniciAdi); 

            if (user == null)
            {
                return Unauthorized("Kullanıcı adı veya şifre hatalı.");
            }

            bool sifreDogruMu = BCrypt.Net.BCrypt.Verify(dto.Sifre, user.SifreHash);

            if (!sifreDogruMu)
            {
                return Unauthorized("Kullanıcı adı veya şifre hatalı.");
            }

            if (user.Durum != KullaniciDurumu.AKTIF)
            {
                return BadRequest("Hesabınız aktif durumda değil. Lütfen sistem yöneticisiyle görüşün.");
            }

            var token = _jwtService.GenerateToken(user.KullaniciAdi, user.RolId);

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
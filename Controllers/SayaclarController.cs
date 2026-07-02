using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KcetasAboneApi.Models;

namespace KcetasAboneApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SayaclarController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SayaclarController(AppDbContext context)
        {
            _context = context;
        }

        // Tüm sayaçları getir
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Sayaclar>>> GetSayaclar()
        {
            return await _context.Sayaclars.ToListAsync();
        }

        // Id'ye göre sayaç getir
        [HttpGet("{id}")]
        public async Task<ActionResult<Sayaclar>> GetSayac(long id)
        {
            var sayac = await _context.Sayaclars.FindAsync(id);

            if (sayac == null)
                return NotFound();

            return sayac;
        }

        // Seri numarasına göre sayaç getir
        [HttpGet("seri/{seriNo}")]
        public async Task<ActionResult<Sayaclar>> GetSayacBySeriNo(string seriNo)
        {
            var sayac = await _context.Sayaclars
                .FirstOrDefaultAsync(x => x.SeriNo == seriNo);

            if (sayac == null)
                return NotFound();

            return sayac;
        }

        [HttpPost]
        public async Task<IActionResult> CreateSayac([FromBody] SayacCreateDto dto)
        {
            // 1. Validasyon: Bu seri no zaten var mı?
            var mevcutSayac = await _context.Sayaclars
                .FirstOrDefaultAsync(s => s.SeriNo == dto.SeriNo);

            if (mevcutSayac != null)
                return BadRequest("Aga bu seri numarasına sahip bir sayaç zaten depoda veya sahada var!");

            // 2. Yeni Sayacı Oluştur
            var yeniSayac = new Sayaclar
            {
                SeriNo = dto.SeriNo,
                TuketimNoktasiId = dto.TuketimNoktasiId,
                Marka = dto.Marka,
                Model = dto.Model,
                Faz = dto.Faz,
                // Eğer çarpan 0 gönderilirse patlamaması için varsayılan 1 alıyoruz (Çarpan 0 olmaz)
                Carpan = dto.Carpan == 0 ? 1M : dto.Carpan, 
                MuhurNo = dto.MuhurNo,
                Durum = dto.Durum,
                Status = "AKTIF", // Silinmemiş, geçerli kayıt
                CreatedAt = DateTime.UtcNow // Oluşturulma zamanı şuan
                // CreatedBy = 1 // Eğer admin logu tutuyorsan burayı açabilirsin
            };

            _context.Sayaclars.Add(yeniSayac);
            await _context.SaveChangesAsync();

            return Ok(yeniSayac);
        }
    }
}

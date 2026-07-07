using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KcetasAboneApi.Models;
using Microsoft.AspNetCore.Authorization;

namespace KcetasAboneApi.Controllers
{   
    //[Authorize(Roles = "1, 5")]
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

            var mevcutSayac = await _context.Sayaclars
                .FirstOrDefaultAsync(s => s.SeriNo == dto.SeriNo);

            if (mevcutSayac != null)
                return BadRequest("Aga bu seri numarasına sahip bir sayaç zaten depoda veya sahada var!");

            var yeniSayac = new Sayaclar
            {
                SeriNo = dto.SeriNo,
                TuketimNoktasiId = dto.TuketimNoktasiId,
                Marka = dto.Marka,
                Model = dto.Model,
                Faz = dto.Faz,

                Carpan = dto.Carpan == 0 ? 1M : dto.Carpan, 
                MuhurNo = dto.MuhurNo,
                Durum = dto.Durum,
                Status = "AKTIF", 
                CreatedAt = DateTime.UtcNow 
            };

            _context.Sayaclars.Add(yeniSayac);
            await _context.SaveChangesAsync();

            return Ok(yeniSayac);
        }
    }
}

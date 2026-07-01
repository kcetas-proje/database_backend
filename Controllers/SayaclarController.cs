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
    }
}

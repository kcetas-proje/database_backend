using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KcetasAboneApi.Models;

namespace KcetasAboneApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IlceController : ControllerBase
    {
        private readonly AppDbContext _context;

        public IlceController(AppDbContext context)
        {
            _context = context;
        }

        // Tüm ilçeleri getir
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ilce>>> GetIlceler()
        {
            return await _context.Ilces.ToListAsync();
        }

        // Id'ye göre ilçe getir
        [HttpGet("{id}")]
        public async Task<ActionResult<Ilce>> GetIlce(int id)
        {
            var ilce = await _context.Ilces.FindAsync(id);

            if (ilce == null)
                return NotFound();

            return ilce;
        }

        // İle göre ilçeleri getir
        [HttpGet("il/{ilId}")]
        public async Task<ActionResult<IEnumerable<Ilce>>> GetIlcelerByIl(short ilId)
        {
            return await _context.Ilces
                .Where(x => x.IlId == ilId)
                .ToListAsync();
        }
    }
}

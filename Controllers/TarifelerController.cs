using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KcetasAboneApi.Models;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace KcetasAboneApi.Controllers
{
    // [Authorize(Roles = "1")]
    [Route("api/[controller]")]
    [ApiController]
    public class TarifelerController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TarifelerController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAktifTarifeler()
        {
            var tarifeler = await _context.Tarifelers.Where(t => t.Aktif).ToListAsync();
            return Ok(tarifeler);
        }

        [HttpPost]
        public async Task<IActionResult> PostTarife([FromBody] Tarifeler yeniTarife)
        {
            yeniTarife.CreatedAt = DateTime.UtcNow; 
            yeniTarife.Aktif = true; 
            
            _context.Tarifelers.Add(yeniTarife);
            await _context.SaveChangesAsync();

            return Ok(new 
            { 
                Mesaj = "Tarife başarıyla eklendi.", 
                TarifeKodu = yeniTarife.TarifeKodu 
            });
        }
    }
}
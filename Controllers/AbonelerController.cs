using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KcetasAboneApi.Models;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System;

namespace KcetasAboneApi.Controllers
{
    // [Authorize(Roles = "1,2")] 
    [Route("api/[controller]")]
    [ApiController]
    public class AbonelerController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AbonelerController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAboneler()
        {
            var aboneler = await _context.Abonelers.ToListAsync();
            return Ok(aboneler);
        }

        [HttpPost]
        public async Task<IActionResult> PostAbone([FromBody] Aboneler yeniAbone)
        {
            yeniAbone.CreatedAt = DateTime.UtcNow; 
            
            _context.Abonelers.Add(yeniAbone);
            await _context.SaveChangesAsync();

            return Ok(new 
            { 
                Mesaj = "Abone başarıyla veritabanına yazıldı.", 
                AboneNo = yeniAbone.AboneNo 
            });
        }
    }
}
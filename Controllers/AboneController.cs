using KcetasAboneApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace KcetasAboneApi.Controllers 
{
    [Route("api/[controller]")]
    [ApiController]
    public class AboneController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AboneController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAboneler()
        {

            var aboneler = await _context.Abones 
                .Where(a => a.Status == "AKTIF")
                .ToListAsync();

            if (!aboneler.Any())
            {
                return NotFound("Sistemde aktif abone bulunamadı.");
            }

            return Ok(aboneler);
        }

        [HttpPost]
        public async Task<IActionResult> YeniAboneEkle([FromBody] Abone yeniAbone)
        {

            yeniAbone.Status = "AKTIF";
            yeniAbone.CreatedAt = DateTime.UtcNow;

            _context.Abones.Add(yeniAbone); 
            await _context.SaveChangesAsync();

            return Ok(yeniAbone);
        }

        // PUT: api/Abone/{id}
        // Mevcut bir abonenin bilgilerini günceller
        [HttpPut("{id}")]
        public async Task<IActionResult> AboneGuncelle(long id, [FromBody] Abone guncelAbone)
        {
            // Önce adamı veritabanında arıyoruz
            var dbAbone = await _context.Abones.FindAsync(id); // "Abones" kısmını kendi DbSet adına göre düzelt
            
            if (dbAbone == null)
            {
                return NotFound("Aga böyle bir abone yok, NPC arıyorsun şu an fr fr.");
            }

            // Güncellenebilir alanları değiştiriyoruz (AboneNo, TCKN falan değişmez, onlar mühürlü)
            dbAbone.Ad = guncelAbone.Ad;
            dbAbone.Soyad = guncelAbone.Soyad;
            dbAbone.Unvan = guncelAbone.Unvan;
            dbAbone.Telefon = guncelAbone.Telefon;
            dbAbone.EPosta = guncelAbone.EPosta;
            dbAbone.IletisimTercihi = guncelAbone.IletisimTercihi;

            // Değişiklikleri veritabanına zımbala
            await _context.SaveChangesAsync();

            return Ok(dbAbone);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> AboneSil(long id)
        {
            var dbAbone = await _context.Abones.FindAsync(id);
            
            if (dbAbone == null)
            {
                return NotFound("Böyle bir abone mevcut değil.");
            }

            dbAbone.Status = "PASIF";
            
            await _context.SaveChangesAsync();

            return Ok(new { mesaj = $"{dbAbone.AboneNo} numaralı abone başarıyla pasife çekildi." });
        }
    }
}
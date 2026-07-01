using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KcetasAboneApi.Models;

namespace KcetasAboneApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IsEmirleriController : ControllerBase
    {
        private readonly AppDbContext _context;

        public IsEmirleriController(AppDbContext context)
        {
            _context = context;
        }

        // Tüm iş emirlerini getir
        [HttpGet]
        public async Task<IActionResult> GetIsEmirleri()
        {
            var isEmirleri = await _context.IsEmirleris
                .Where(x => x.Status == "AKTIF")
                .ToListAsync();

            if (!isEmirleri.Any())
            {
                return NotFound("Sistemde aktif iş emri bulunamadı.");
            }

            return Ok(isEmirleri);
        }

        // Id'ye göre iş emri getir
        [HttpGet("{id}")]
        public async Task<IActionResult> GetIsEmri(long id)
        {
            var isEmri = await _context.IsEmirleris.FindAsync(id);

            if (isEmri == null)
            {
                return NotFound("İş emri bulunamadı.");
            }

            return Ok(isEmri);
        }

        // Duruma göre iş emirlerini getir
        [HttpGet("durum/{durum}")]
        public async Task<IActionResult> GetDurumaGoreIsEmirleri(string durum)
        {
            var isEmirleri = await _context.IsEmirleris
                .Where(x => x.Durum == durum && x.Status == "AKTIF")
                .ToListAsync();

            if (!isEmirleri.Any())
            {
                return NotFound("Bu duruma ait iş emri bulunamadı.");
            }

            return Ok(isEmirleri);
        }

        // Yeni iş emri ekle
        [HttpPost]
        public async Task<IActionResult> YeniIsEmriEkle([FromBody] IsEmirleri yeniIsEmri)
        {
            yeniIsEmri.Status = "AKTIF";
            yeniIsEmri.CreatedAt = DateTime.UtcNow;

            _context.IsEmirleris.Add(yeniIsEmri);
            await _context.SaveChangesAsync();

            return Ok(yeniIsEmri);
        }

        // İş emrini güncelle
        [HttpPut("{id}")]
        public async Task<IActionResult> IsEmriGuncelle(long id, [FromBody] IsEmirleri guncelIsEmri)
        {
            var dbIsEmri = await _context.IsEmirleris.FindAsync(id);

            if (dbIsEmri == null)
            {
                return NotFound("İş emri bulunamadı.");
            }

            dbIsEmri.IsEmriNo = guncelIsEmri.IsEmriNo;
            dbIsEmri.TuketimNoktasiId = guncelIsEmri.TuketimNoktasiId;
            dbIsEmri.SayacId = guncelIsEmri.SayacId;
            dbIsEmri.Tip = guncelIsEmri.Tip;
            dbIsEmri.Oncelik = guncelIsEmri.Oncelik;
            dbIsEmri.PlanlananTarih = guncelIsEmri.PlanlananTarih;
            dbIsEmri.AtananKullaniciId = guncelIsEmri.AtananKullaniciId;
            dbIsEmri.Durum = guncelIsEmri.Durum;
            dbIsEmri.SahaSonucu = guncelIsEmri.SahaSonucu;
            dbIsEmri.Gerekce = guncelIsEmri.Gerekce;
            dbIsEmri.MuhurNo = guncelIsEmri.MuhurNo;
            dbIsEmri.TutanakNo = guncelIsEmri.TutanakNo;
            dbIsEmri.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(dbIsEmri);
        }

        // Soft Delete
        [HttpDelete("{id}")]
        public async Task<IActionResult> IsEmriSil(long id)
        {
            var dbIsEmri = await _context.IsEmirleris.FindAsync(id);

            if (dbIsEmri == null)
            {
                return NotFound("İş emri bulunamadı.");
            }

            dbIsEmri.Status = "PASIF";
            dbIsEmri.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mesaj = $"{dbIsEmri.IsEmriNo} numaralı iş emri pasif duruma alındı."
            });
        }
    }
}

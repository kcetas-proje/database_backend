using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace KcetasAboneApi.Controllers
{
    //[Authorize(Roles = "1, 7")]
    [ApiController]
    [Route("api/entegrator")]
    public class EntegratorController : ControllerBase
    {
        [HttpPost("gib-gonder")]
        public IActionResult GibFaturaGonder([FromBody] object faturaPayload)
        {

            var random = new Random().Next(1, 100);

            if (random <= 60) 
                return Ok(new { Code = "200", Status = "BAŞARILI", Message = "Fatura GİB'e başarıyla iletildi." });

            if (random <= 70) 
                return BadRequest(new { Code = "400", Status = "SEMA_HATASI", Message = "XML/JSON payload geçerli değil." });

            if (random <= 80) 
                return NotFound(new { Code = "404", Status = "ALICI_BULUNAMADI", Message = "Gönderilen VKN sistemde kayıtlı değil." });

            if (random <= 90) 
                return Conflict(new { Code = "409", Status = "MUKERRER_BELGE", Message = "Bu fatura numarası daha önce kullanılmış." });

            return StatusCode(504, new { Code = "504", Status = "ZAMAN_ASIMI", Message = "GİB sunucularından yanıt alınamadı." });
        }
    }
}
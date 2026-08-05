using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KcetasAboneApi.Models 
{
    [Table("bildirimler")]
    public class Bildirim
    {
        [Key]
        public int BildirimId { get; set; }


        [Required]
        public int KullaniciId { get; set; } 

        [Required]
        [MaxLength(200)]
        public string Baslik { get; set; } = null!;


        [Required]
        public string Icerik { get; set; } = null!;


        public bool OkunduMu { get; set; } = false;


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
            
        [Column("is_emri_id")]
        public long? IsEmriId { get; set; }
    }
}
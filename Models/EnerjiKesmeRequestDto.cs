using System.ComponentModel.DataAnnotations;

namespace KcetasAboneApi.Models.Dtos;
using System.Text.Json.Serialization;

public class EnerjiKesmeRequestDto
{
    [Required]
    public long JobId { get; set; }

    [Required]
    public string SeriNo { get; set; } = null!;

    [Required(ErrorMessage = "Kesme noktası boş geçilemez!")]
    public string KesmeNoktasi { get; set; } = null!;

    [Required(ErrorMessage = "Kesme nedeni boş geçilemez!")]
    public string KesmeNedeni { get; set; } = null!; 

    [Range(0, double.MaxValue, ErrorMessage = "Son endeks negatif olamaz!")]
    public decimal SonEndeks { get; set; }

    [JsonPropertyName("aboDur")]
    public string? AboneDurumu { get; set; }

    [JsonPropertyName("SycDur")]
    public string? SayacDurumu { get; set; } 

    [MaxLength(500)]
    public string? Aciklama { get; set; }

    [Required]
    public string MuhurNo { get; set; } = null!;

    [Required]
    public long IslemYapanKullaniciId { get; set; }
}
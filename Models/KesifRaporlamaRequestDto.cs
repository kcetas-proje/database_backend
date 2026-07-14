using System.ComponentModel.DataAnnotations;

namespace KcetasAboneApi.Models.Dtos;

public class KesifRaporlamaRequestDto
{
    [Required]
    public long JobId { get; set; }

    [Required]
    public string PanoDirekNo { get; set; } = null!; 

    [Required]
    public string KesifSonucu { get; set; } = null!;

    [Required]
    public string YapiTesisTipi { get; set; } = null!; 

  [MaxLength(100)]
    public string? HatMesafesi { get; set; } 

    [MaxLength(100)]
    public string? TalepGucu { get; set; } 

    [MaxLength(500)]
    public string? IncelemeNotu { get; set; } 

    [Required]
    public long IslemYapanKullaniciId { get; set; }
}
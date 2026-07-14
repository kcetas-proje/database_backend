using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KcetasAboneApi.Models.Dtos;

public class SayacArizaRequestDto
{
    [Required]
    public long JobId { get; set; }

    [Required]
    public string SeriNo { get; set; } = null!;

    [Required]
    public string ArizaTipi { get; set; } = null!; 

    [JsonPropertyName("arizaDrc")]
    [Required]
    public string ArizaDerecesi { get; set; } = null!; 
    [Required]
    public string SahaSonucu { get; set; } = null!; 

    public string? TutanakNo { get; set; } 

    public decimal Aktif { get; set; }
    public decimal Gunduz { get; set; }
    public decimal Puant { get; set; }
    public decimal Gece { get; set; }
    public decimal Induktif { get; set; }
    public decimal Kapasitif { get; set; }
    public decimal Demand { get; set; }

    [Required]
    public long IslemYapanKullaniciId { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace KcetasAboneApi.Models.Dtos;

public class SayacYeniBaglantiRequestDto
{
    [Required]
    public long JobId { get; set; }

    [Required]
    public string SeriNo { get; set; } = null!;

    [Required]
    public string BaglantiTipi { get; set; } = null!; 

    [Required]
    public string SayacFazi { get; set; } = null!; 

    [Required]
    public string YeniMuhurNo { get; set; } = null!;

    public decimal IlkEndeks { get; set; } 

    [Range(1900, 2100)]
    public int DamgaYili { get; set; }

    [Required]
    public long IslemYapanKullaniciId { get; set; }
}
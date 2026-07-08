using System.ComponentModel.DataAnnotations;

namespace KcetasAboneApi.Models.Dtos;

public class IsEmriCreateDto
{
    [Required]
    public long TuketimNoktasiId { get; set; }
    
    public long? SayacId { get; set; }
    
    public long? AtananKullaniciId { get; set; }

    [Required]
    public string Tip { get; set; } = null!;

    public string Oncelik { get; set; } = "NORMAL";
    
    public string Durum { get; set; } = "ACIK";
}
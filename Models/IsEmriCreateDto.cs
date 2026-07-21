using System.ComponentModel.DataAnnotations;

namespace KcetasAboneApi.Models.Dtos;

public class IsEmriCreateDto
{
    [Required]
    public long TuketimNoktasiId { get; set; }
    
    public long? SayacId { get; set; }
    
    public long? AtananKullaniciId { get; set; }

    [Required]
    public IsEmriTipi Tip { get; set; }

    public string Oncelik { get; set; } = "NORMAL";
    
    public IsEmriDurumu Durum { get; set; } = IsEmriDurumu.ACIK;
}
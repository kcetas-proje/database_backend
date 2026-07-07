
using System.ComponentModel.DataAnnotations;

namespace KcetasAboneApi.Models; 

public class AboneCreateDto
{
    [Required]
    public string AboneTipi { get; set; } = null!; 

    public string? Ad { get; set; }
    public string? Soyad { get; set; }
    public string? Unvan { get; set; } // Kurumsal için
    public string? Tckn { get; set; }
    public string? Vkn { get; set; } // Kurumsal için
    
    [Required]
    public string Telefon { get; set; } = null!;
}
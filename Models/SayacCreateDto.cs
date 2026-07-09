namespace KcetasAboneApi.Models;

public class SayacCreateDto
{
    public string SeriNo { get; set; } = null!;
    public long? TuketimNoktasiId { get; set; }
    public string? Marka { get; set; }
    public string? Model { get; set; }
    public int UretimYili { get; set; }
    public string? Faz { get; set; } 
    public decimal Carpan { get; set; } 
    public string? MuhurNo { get; set; }
    public string Durum { get; set; } = null!; 
}
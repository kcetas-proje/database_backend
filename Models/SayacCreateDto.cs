namespace KcetasAboneApi.Models;

public class SayacCreateDto
{
    public string SeriNo { get; set; } = null!;
    public long? TuketimNoktasiId { get; set; }
    public string? Marka { get; set; }
    public string? Model { get; set; }
    public int UretimYili { get; set; }
    public Faz? Faz { get; set; } 
    public decimal Carpan { get; set; } 
    public string? MuhurNo { get; set; }
    public SayacDurumu Durum { get; set; } = SayacDurumu.DEPODA; 
}
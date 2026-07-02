namespace KcetasAboneApi.Models;

public class EndeksOkumaCreateDto
{
    public long SayacId { get; set; }
    public long? IsEmriId { get; set; }
    public long? SozlesmeId { get; set; }
    public decimal YeniEndeks { get; set; }
    public decimal? OncekiEndeks { get; set; } 
    public string? OkumaTipi { get; set; }
    public string? OkumaKaynagi { get; set; }
    public string? Donem { get; set; }
    public DateTime? OkumaZamani { get; set; }
    public long? KullaniciId { get; set; }
}
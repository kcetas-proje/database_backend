namespace KcetasAboneApi.Models;

public class FaturaKalemiCreateDto
{
    public long FaturaId { get; set; }
    public string KalemTipi { get; set; } = null!;
    public string? Aciklama { get; set; }
    public decimal Miktar { get; set; }
    public decimal? BirimFiyat { get; set; }
    public decimal Tutar { get; set; }
}
namespace KcetasAboneApi.Models
{
    public class FaturaKalemiUpdateDto
{
    public int FaturaKalemId { get; set; }
    public string Aciklama { get; set; }
    public decimal Miktar { get; set; }
    public decimal BirimFiyat { get; set; }
    public decimal Tutar { get; set; }
}
}
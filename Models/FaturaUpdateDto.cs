namespace KcetasAboneApi.Models
{

public class FaturaUpdateDto
{
    public int FaturaId { get; set; }
    public string FaturaNo { get; set; }
    public decimal ToplamTutar { get; set; }
    public FaturaDurumu Durum { get; set; }
    public DateOnly SonOdemeTarihi { get; set; }
}
}
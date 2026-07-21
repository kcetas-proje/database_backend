namespace KcetasAboneApi.Models;

public class FaturaCreateDto
{
    public long SozlesmeId { get; set; }
    public long? IsEmriId { get; set; } 
    public FaturaTipi FaturaTipi { get; set; }
    public string Donem { get; set; }
    public decimal IlkEndeks { get; set; }
    public decimal SonEndeks { get; set; }
    public decimal TuketimKwh { get; set; }
    public decimal ReaktifEnduktif { get; set; }
    public decimal ReaktifKapasitif { get; set; }
    public decimal ToplamTutar { get; set; }
    public FaturaDurumu Durum { get; set; } = FaturaDurumu.ODENMEDI;
    public DateTime OkumaZamani { get; set; } 
    public long KullaniciId { get; set; }
}
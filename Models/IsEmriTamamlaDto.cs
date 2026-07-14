namespace KcetasAboneApi.Models;

public class CompleteJobRequestDto
{
    public long JobId { get; set; }
    public string? Notes { get; set; }
    public string? MuhurNo { get; set; }
    public decimal SonEndeks { get; set; }

    public decimal? Gunduz { get; set; }
    public decimal? Puant { get; set; }
    public decimal? Gece { get; set; }
    public decimal? Induktif { get; set; }
    public decimal? Kapasitif { get; set; }
    public decimal? Demand { get; set; }
    public string? SahaSonucu { get; set; }

    public long IslemYapanKullaniciId { get; set; }
}
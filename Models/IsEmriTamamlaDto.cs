namespace KcetasAboneApi.Models;

public class CompleteJobRequestDto
{
    public long JobId { get; set; }
    public string? Notes { get; set; }
    public string? MuhurNo { get; set; }
    public decimal SonEndeks { get; set; }

    public long IslemYapanKullaniciId { get; set; }
}
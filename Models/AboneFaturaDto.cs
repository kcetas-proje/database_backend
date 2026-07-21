namespace KcetasAboneApi.Models;

public class AboneFaturaDto
{
    public long FaturaId { get; set; }

    public string FaturaNo { get; set; } = string.Empty;

    public string Donem { get; set; } = string.Empty;

    public DateOnly FaturaTarihi { get; set; }

    public decimal ToplamTutar { get; set; }

    public string Durum { get; set; } = string.Empty;
}

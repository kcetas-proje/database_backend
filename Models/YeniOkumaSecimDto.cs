namespace KcetasAboneApi.Models.Dtos;

public class YeniOkumaSecimDto
{
    public long SozlesmeId { get; set; }
    public string SozlesmeNo { get; set; } = string.Empty;
    public long TuketimNoktasiId { get; set; }
    public string TekilKod { get; set; } = string.Empty;
    public string Adres { get; set; } = string.Empty;
    public long SayacId { get; set; }
    public string SayacSeriNo { get; set; } = string.Empty;
    public decimal? SonEndeks { get; set; }
    public string? Donem { get; set; }
}

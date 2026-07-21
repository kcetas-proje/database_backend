namespace KcetasAboneApi.Models.Dtos; // Kendi namespace'ine göre ayarla

public class EndeksOkumaDetailDto
{
    public long OkumaId { get; set; }
    public long SayacId { get; set; }
    public long? IsEmriId { get; set; }
    public long? SozlesmeId { get; set; }
    public long? AboneId { get; set; }
    public OkumaTipi? OkumaTipi { get; set; }
    public OkumaKaynagi? OkumaKaynagi { get; set; }
    public decimal? OncekiEndeks { get; set; }
    public decimal YeniEndeks { get; set; }
    public string? Donem { get; set; }
    public DateTime? OkumaZamani { get; set; }
    public long? KullaniciId { get; set; }

    public string SeriNo { get; set; } = null!;
    public string MarkaModel { get; set; } = null!;
    public string Mahalle { get; set; } = null!;
    public string AcikAdres { get; set; } = null!;
    public string TuketiciGrubu { get; set; } = null!;
}
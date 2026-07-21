namespace KcetasAboneApi.Models.Dtos;

public class TuketimNoktasiDetailDto
{
    public long TuketimNoktasiId { get; set; }
    public string TekilKod { get; set; } = null!;
    public string Mahalle { get; set; } = null!;
    public string AcikAdres { get; set; } = null!;
    public decimal BaglantiGucuKw { get; set; }
    public string TuketiciGrubu { get; set; } = null!;
    public BaglantiDurumu BaglantiDurumu { get; set; }
    public string Status { get; set; } = null!;

    // 🎯 İlişkili Tablolardan Çekilecek VIP Veriler
    public string IlceAdi { get; set; } = "Bilinmiyor";
    public string AktifSayacSeriNo { get; set; } = "SAYAÇ YOK";
    public long? AktifAboneId { get; set; }
    public string AktifSozlesmeNo { get; set; } = "SÖZLEŞME YOK";
}
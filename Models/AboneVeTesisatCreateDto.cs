public class AboneVeTesisatCreateDto
{
    // 👤 Abone Bilgileri
    public string Ad { get; set; } = null!;
    public string Soyad { get; set; } = null!;
    public string TcNo { get; set; } = null!;
    public string Telefon { get; set; } = null!;

    // ⚡ Tesisat (Tüketim Noktası) Bilgileri
    public int IlceId { get; set; }
    public string Mahalle { get; set; } = null!;
    public string BinaNo { get; set; } = null!;
    public string BagimsizBolumNo { get; set; } = null!;
    public string AcikAdres { get; set; } = null!;
    public decimal BaglantiGucuKw { get; set; }
    public string TuketiciGrubu { get; set; } = "MESKEN";
}
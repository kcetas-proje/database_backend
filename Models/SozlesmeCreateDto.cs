public class SozlesmeCreateDto
{
    public long TuketimNoktasiId { get; set; }
    public string? SozlesmeNo { get; set; }
    public string? Ad { get; set; }
    public string? Soyad { get; set; }
    public string? Tckn { get; set; }
    public string? SozlesmeTipi { get; set; }
    public DateTime BaslangicTarihi { get; set; }
    public string? TarifeGrubu { get; set; }
    public decimal GuvenceBedeli { get; set; }
}
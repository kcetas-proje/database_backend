namespace KcetasAboneApi.Models;

public class AboneListDto
{
    public long AboneId { get; set; }
    public string AboneNo { get; set; } = string.Empty;
    public AboneTipi AboneTipi { get; set; }
    public string? Ad { get; set; }
    public string? Soyad { get; set; }
    public string? Unvan { get; set; }
    public string? Tckn { get; set; }
    public string? Vkn { get; set; }
    public string? Telefon { get; set; }
}

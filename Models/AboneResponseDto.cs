namespace KcetasAboneApi.Models;

public class AboneResponseDto
{
    public long AboneId { get; set; }

    public string AboneNo { get; set; } = string.Empty;

    public AboneTipi AboneTipi { get; set; }

    public string? Ad { get; set; }

    public string? Soyad { get; set; }

    public string? Unvan { get; set; }

    public string? Telefon { get; set; }

    public string? EPosta { get; set; }
    
    public string Status { get; set; } = string.Empty;
}

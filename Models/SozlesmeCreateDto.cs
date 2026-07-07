using System;

namespace KcetasAboneApi.Models; 

public class SozlesmeCreateDto
{
    public long AboneId { get; set; } 

    public long TuketimNoktasiId { get; set; } 
    
    public long TarifeId { get; set; } 

    public string? SozlesmeNo { get; set; }
    public string SozlesmeTipi { get; set; } = null!;
    public DateTime BaslangicTarihi { get; set; }
    public decimal GuvenceBedeli { get; set; }
}
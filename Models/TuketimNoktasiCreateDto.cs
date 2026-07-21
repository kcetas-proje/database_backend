using System.ComponentModel.DataAnnotations;

namespace KcetasAboneApi.Models.Dtos;

public class TuketimNoktasiCreateDto
{

    [Required]
    public int IlceId { get; set; }

    [Required]
    public string Mahalle { get; set; } = null!;

    public string? BinaNo { get; set; }
    
    public string? BagimsizBolumNo { get; set; }

    [Required]
    public string AcikAdres { get; set; } = null!;

    public decimal? KoordinatLat { get; set; }
    
    public decimal? KoordinatLon { get; set; }

    [Required]
    public decimal BaglantiGucuKw { get; set; }

    [Required]
    public string TuketiciGrubu { get; set; } = null!; // "MESKEN", "TICARETHANE" vs.

    public BaglantiDurumu BaglantiDurumu { get; set; } = BaglantiDurumu.BAGLANABILIR;
}
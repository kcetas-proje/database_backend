using System.ComponentModel.DataAnnotations;

namespace KcetasAboneApi.Models.Dtos;

public class SayacDegisimiRequestDto
{
    [Required(ErrorMessage = "İş emri ID'si zorunludur.")]
    public long JobId { get; set; }

    // 🔴 SÖKÜLEN SAYAÇ BİLGİLERİ
    [Required(ErrorMessage = "Sökülen sayacın seri nosu zorunludur.")]
    public string SokulenSeriNo { get; set; } = null!;
    
    public decimal SokulenAktif { get; set; }
    public decimal SokulenGunduz { get; set; }
    public decimal SokulenPuant { get; set; }
    public decimal SokulenGece { get; set; }
    public decimal SokulenInduktif { get; set; }
    public decimal SokulenKapasitif { get; set; }
    public decimal SokulenDemand { get; set; }

    // 🟢 TAKILAN YENİ SAYAÇ BİLGİLERİ
    [Required(ErrorMessage = "Yeni sayacın seri nosu zorunludur.")]
    public string YeniSeriNo { get; set; } = null!;
    
    [Required(ErrorMessage = "Yeni kelepçe mühür no zorunludur!")]
    public string YeniMuhurNo { get; set; } = null!;
    
    public decimal YeniIlkEndeks { get; set; }
    
    [Range(1900, 2100, ErrorMessage = "Damga yılı mantıklı bir tarih olmalı!")]
    public int DamgaYili { get; set; } = DateTime.UtcNow.Year;

    public string? AkimTrafosuSeriNo { get; set; }
    public string? AkimTrafosuMarka { get; set; }
    public string? GerilimTrafosuSeriNo { get; set; }
    public string? GerilimTrafosuMarka { get; set; }

    [Required]
    public long IslemYapanKullaniciId { get; set; }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace KcetasAboneApi.Models;

public partial class IsEmirleri
{
    public long IsEmriId { get; set; }

    public string IsEmriNo { get; set; } = null!;

    public long TuketimNoktasiId { get; set; }

    public long? SayacId { get; set; }

    public string Tip { get; set; } = null!;

    public string Oncelik { get; set; } = null!;

    public DateTime? PlanlananTarih { get; set; }

    public long? AtananKullaniciId { get; set; }

    [Column("ad_kodu")]
public string? AdKodu { get; set; }

    [Column("sd_kodu")]
    public string? SdKodu { get; set; }

    [Column("okuma_sirasi")]
    public int? OkumaSirasi { get; set; }

    [Column("tamamlanma_orani")]
    public decimal? TamamlanmaOrani { get; set; }

    public string Durum { get; set; } = null!;

    public string? SahaSonucu { get; set; }

    public string? Gerekce { get; set; }

    public string? MuhurNo { get; set; }

    public string? TutanakNo { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Kullanicilar? AtananKullanici { get; set; }

    public virtual ICollection<EndeksOkuma> EndeksOkumas { get; set; } = new List<EndeksOkuma>();

    public virtual Sayaclar? Sayac { get; set; }

    public virtual TuketimNoktasi TuketimNoktasi { get; set; } = null!;
}

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KcetasAboneApi.Models;

[Table("is_emirleri")]
public partial class IsEmirleri
{
    [Key]
    [Column("is_emri_id")]
    public long IsEmriId { get; set; }

    [Column("is_emri_no")]
    public string IsEmriNo { get; set; } = null!;

    [Column("tuketim_noktasi_id")]
    public long TuketimNoktasiId { get; set; }

    [Column("sayac_id")]
    public long? SayacId { get; set; }

    [Column("tip")]
    public string Tip { get; set; } = null!;

    [Column("oncelik")]
    public string Oncelik { get; set; } = "NORMAL";

    [Column("planlanan_tarih")]
    public DateTime? PlanlananTarih { get; set; }

    [Column("atanan_kullanici_id")]
    public long? AtananKullaniciId { get; set; }

    [Column("durum")]
    public string Durum { get; set; } = "ACIK";

    [Column("saha_sonucu")]
    public string? SahaSonucu { get; set; }

    [Column("gerekce")]
    public string? Gerekce { get; set; }

    [Column("muhur_no")]
    public string? MuhurNo { get; set; }

    [Column("tutanak_no")]
    public string? TutanakNo { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    public virtual Kullanicilar? AtananKullanici { get; set; }

    public virtual ICollection<EndeksOkuma> EndeksOkumas { get; set; } = new List<EndeksOkuma>();

    public virtual Sayaclar? Sayac { get; set; }

    public virtual TuketimNoktasi TuketimNoktasi { get; set; } = null!;
}

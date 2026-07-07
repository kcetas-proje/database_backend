using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace KcetasAboneApi.Models;

[Table("sayaclar")]
public partial class Sayaclar
{
    [Key]
    [Column("sayac_id")]
    public long SayacId { get; set; }

    [Column("seri_no")]
    public string SeriNo { get; set; } = null!;

    [Column("tuketim_noktasi_id")]
    public long? TuketimNoktasiId { get; set; }

    [Column("marka")]
    public string? Marka { get; set; }

    [Column("model")]
    public string? Model { get; set; }

    [Column("faz")]
    public string? Faz { get; set; }

    [Column("carpan")]
    public decimal Carpan { get; set; } = 1;

    [Column("muhur_no")]
    public string? MuhurNo { get; set; }

    [Column("durum")]
    public string Durum { get; set; } = "DEPODA";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("created_by")]
    public long? CreatedBy { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("updated_by")]
    public long? UpdatedBy { get; set; }

    [JsonIgnore]
    [ForeignKey("CreatedBy")]
    public virtual Kullanicilar? CreatedByNavigation { get; set; }

    [JsonIgnore]
    [ForeignKey("UpdatedBy")]
    public virtual Kullanicilar? UpdatedByNavigation { get; set; }

    public virtual ICollection<EndeksOkuma> EndeksOkumas { get; set; } = new List<EndeksOkuma>();

    public virtual ICollection<IsEmirleri> IsEmirleris { get; set; } = new List<IsEmirleri>();

    public virtual TuketimNoktasi? TuketimNoktasi { get; set; }

}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace KcetasAboneApi.Models;

[Table("tuketim_noktasi")]
public partial class TuketimNoktasi
{
    [Key] 
    [Column("tuketim_noktasi_id")]
    public long TuketimNoktasiId { get; set; }

    [Column("tekil_kod")]
    public string TekilKod { get; set; } = null!;

    [Column("ilce_id")]
    public int IlceId { get; set; }

    [Column("mahalle")]
    public string Mahalle { get; set; } = null!;

    [Column("bina_no")]
    public string? BinaNo { get; set; }

    [Column("bagimsiz_bolum_no")]
    public string? BagimsizBolumNo { get; set; }

    [Column("acik_adres")]
    public string AcikAdres { get; set; } = null!;

    [Column("koordinat_lat")]
    public decimal? KoordinatLat { get; set; }

    [Column("koordinat_lon")]
    public decimal? KoordinatLon { get; set; }

    [Column("baglanti_gucu_kw")]
    public decimal BaglantiGucuKw { get; set; }

    [Column("tuketici_grubu")]
    public string TuketiciGrubu { get; set; } = null!;

    [Column("baglanti_durumu")]
    public BaglantiDurumu BaglantiDurumu { get; set; }

    [Column("status")]
    public string Status { get; set; } = "AKTIF";

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

    [JsonIgnore]
    [ForeignKey("IlceId")] 
    public virtual Ilce? Ilce { get; set; }

    [JsonIgnore]
    public virtual ICollection<IsEmirleri> IsEmirleris { get; set; } = new List<IsEmirleri>();

    [JsonIgnore]
    public virtual ICollection<Sayaclar> Sayaclars { get; set; } = new List<Sayaclar>();

    [JsonIgnore]
    public virtual ICollection<Sozlesmeler> Sozlesmelers { get; set; } = new List<Sozlesmeler>();

}
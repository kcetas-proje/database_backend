using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization; 

namespace KcetasAboneApi.Models;

[Table("endeks_okuma")]
public partial class EndeksOkuma
{
    [Key]
    [Column("okuma_id")]
    public long OkumaId { get; set; }

    [Column("sayac_id")]
    public long SayacId { get; set; }

    [Column("is_emri_id")]
    public long? IsEmriId { get; set; }

    [Column("sozlesme_id")]
    public long? SozlesmeId { get; set; }

    [Column("okuma_tipi")]
    public OkumaTipi OkumaTipi { get; set; }

    [Column("okuma_kaynagi")]
    public OkumaKaynagi OkumaKaynagi { get; set; }

    [Column("gunduz_endeks")]
    [NotMapped]
    public decimal? GunduzEndeks { get; set; }

    [Column("puant_endeks")]
    [NotMapped]
    public decimal? PuantEndeks { get; set; }

    [Column("gece_endeks")]
    [NotMapped]
    public decimal? GeceEndeks { get; set; }

    [Column("induktif_endeks")]
    [NotMapped]
    public decimal? InduktifEndeks { get; set; }

    [Column("kapasitif_endeks")]
    [NotMapped]
    public decimal? KapasitifEndeks { get; set; }

    [Column("demand")]
    [NotMapped]
    public decimal? Demand { get; set; }

    [Column("onceki_endeks")]
    public decimal? OncekiEndeks { get; set; }

    [Column("yeni_endeks")]
    public decimal YeniEndeks { get; set; }

    [Column("donem")]
    public string? Donem { get; set; }

    [Column("okuma_zamani")]
    public DateTime OkumaZamani { get; set; }

    [Column("kullanici_id")]
    public long? KullaniciId { get; set; }

    [Column("okunamama_nedeni")]
    public string? OkunamamaNedeni { get; set; }

    [Column("dogrulama_durumu")]
    public DogrulamaDurumu DogrulamaDurumu { get; set; }

    [Column("anomali_mi")]
    public bool AnomaliMi { get; set; }

    [Column("status")]
    public string Status { get; set; } = "AKTIF";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonIgnore]
    public virtual ICollection<Fatura> Faturas { get; set; } = new List<Fatura>();

    [JsonIgnore]
    public virtual IsEmirleri? IsEmri { get; set; }

    [JsonIgnore]
    public virtual Kullanicilar? Kullanici { get; set; }

    [JsonIgnore]
    public virtual Sayaclar? Sayac { get; set; }

    [JsonIgnore]
    public virtual Sozlesmeler? Sozlesme { get; set; }
}
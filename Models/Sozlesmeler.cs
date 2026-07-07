using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization; 

namespace KcetasAboneApi.Models;

[Table("sozlesmeler")] 
public partial class Sozlesmeler
{
    [Key] 
    [Column("sozlesme_id")]
    public long SozlesmeId { get; set; }

    [Column("sozlesme_no")]
    public string SozlesmeNo { get; set; } = null!;
    
    [Column("tuketim_noktasi_id")]
    public long TuketimNoktasiId { get; set; }

    [Column("abone_id")]
    public long AboneId { get; set; }

    [Column("tarife_id")]
    public long TarifeId { get; set; }

    [Column("sozlesme_tipi")]
    public string SozlesmeTipi { get; set; } = null!;

    [Column("durum")]
    public string Durum { get; set; } = "AKTIF";

    [Column("baslangic_tarihi")]
    public DateOnly BaslangicTarihi { get; set; }

    [Column("bitis_tarihi")]
    public DateOnly? BitisTarihi { get; set; }

    [Column("guvence_bedeli")]
    public decimal GuvenceBedeli { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [JsonIgnore]
    [ForeignKey("TuketimNoktasiId")]
    public virtual TuketimNoktasi? TuketimNoktasi { get; set; }

    [JsonIgnore]
    [ForeignKey("AboneId")]
    public virtual Aboneler? Abone { get; set; }

    [JsonIgnore]
    [ForeignKey("TarifeId")] 
    public virtual Tarifeler? Tarife { get; set; }
    
    [JsonIgnore]
    public virtual ICollection<EndeksOkuma> EndeksOkumas { get; set; } = new List<EndeksOkuma>();
    
    [JsonIgnore]
    public virtual ICollection<Fatura> Faturas { get; set; } = new List<Fatura>();
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace KcetasAboneApi.Models;

[Table("kullanicilar")]
public partial class Kullanicilar
{
    [Key]
    [Column("kullanici_id")]
    public long KullaniciId { get; set; }

    [Column("rol_id")]
    public short RolId { get; set; }

    [Column("ad_soyad")]
    public string AdSoyad { get; set; } = null!;

    [Column("e_posta")]
    public string EPosta { get; set; } = null!;

    [Column("kullanici_adi")]
    public string KullaniciAdi { get; set; } = null!;

    [Column("sifre_hash")]
    public string SifreHash { get; set; } = null!;

    [Column("durum")]
    public KullaniciDurumu Durum { get; set; } = KullaniciDurumu.AKTIF;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
    
    [NotMapped] 
    public virtual ICollection<Sayaclar> SayaclarCreatedByNavigations { get; set; } = new List<Sayaclar>();
    
    [NotMapped]
    public virtual ICollection<Sayaclar> SayaclarUpdatedByNavigations { get; set; } = new List<Sayaclar>();
    
    [NotMapped]
    public virtual ICollection<Sozlesmeler> SozlesmelerCreatedByNavigations { get; set; } = new List<Sozlesmeler>();
    
    [NotMapped]
    public virtual ICollection<Sozlesmeler> SozlesmelerUpdatedByNavigations { get; set; } = new List<Sozlesmeler>();
    
    [NotMapped]
    public virtual ICollection<TuketimNoktasi> TuketimNoktasiCreatedByNavigations { get; set; } = new List<TuketimNoktasi>();
    
    [NotMapped]
    public virtual ICollection<TuketimNoktasi> TuketimNoktasiUpdatedByNavigations { get; set; } = new List<TuketimNoktasi>();

    [JsonIgnore]
    public virtual Roller? Rol { get; set; }

    [JsonIgnore]
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    [JsonIgnore]
    public virtual ICollection<EndeksOkuma> EndeksOkumas { get; set; } = new List<EndeksOkuma>();

    [JsonIgnore]
    public virtual ICollection<IsEmirleri> IsEmirleris { get; set; } = new List<IsEmirleri>();
}
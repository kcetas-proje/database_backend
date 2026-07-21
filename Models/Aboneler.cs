using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // BU ÇOK KRİTİK
using System.Text.Json.Serialization;

namespace KcetasAboneApi.Models;

[Table("aboneler")] // Tablo adını netleştirdik
public partial class Aboneler
{
    [Key]
    [Column("abone_id")]
    public long AboneId { get; set; }
    
    [Column("abone_no")]
    public string AboneNo { get; set; } = null!;
    
    [Column("abone_tipi")]
    public AboneTipi AboneTipi { get; set; }
    
    [Column("ad")]
    public string? Ad { get; set; }
    
    [Column("soyad")]
    public string? Soyad { get; set; }
    
    [Column("unvan")]
    public string? Unvan { get; set; }
    
    [Column("tckn")]
    public string? Tckn { get; set; }
    
    [Column("vkn")]
    public string? Vkn { get; set; }
    
    [Column("telefon")]
    public string? Telefon { get; set; }
    
    [Column("e_posta")]
    public string? EPosta { get; set; }
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
    
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [JsonIgnore]
    public virtual ICollection<Sozlesmeler> Sozlesmelers { get; set; } = new List<Sozlesmeler>();
}
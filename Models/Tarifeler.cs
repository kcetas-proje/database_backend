using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KcetasAboneApi.Models;

[Table("tarifeler")]
public partial class Tarifeler
{
    [Key]
    [Column("tarife_id")]
    public long TarifeId { get; set; }
    
    [Column("tarife_kodu")]
    public string TarifeKodu { get; set; } = null!;
    
    [Column("tarife_adi")]
    public string TarifeAdi { get; set; } = null!;
    
    [Column("gunduz_birim_fiyat")]
    public decimal GunduzBirimFiyat { get; set; }
    
    [Column("puant_birim_fiyat")]
    public decimal? PuantBirimFiyat { get; set; }
    
    [Column("gece_birim_fiyat")]
    public decimal? GeceBirimFiyat { get; set; }

    [Column("induktif_birim_fiyat")]
    public decimal? InduktifBirimFiyat { get; set; }

    [Column("kapasitif_birim_fiyat")]
    public decimal? KapasitifBirimFiyat { get; set; }

    [Column("demand_birim_fiyat")]
    public decimal? DemandBirimFiyat { get; set; }
    
    [Column("kdv_orani")]
    public decimal KdvOrani { get; set; }
    
    [Column("dagitim_bedeli")]
    public decimal DagitimBedeli { get; set; }
    
    [Column("aktif")]
    public bool Aktif { get; set; }
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
    
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    
}
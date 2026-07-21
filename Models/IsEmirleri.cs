using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

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
    public IsEmriTipi Tip { get; set; }

    [Column("oncelik")]
    public string Oncelik { get; set; } = "NORMAL";

    [Column("planlanan_tarih")]
    public DateTime? PlanlananTarih { get; set; }

    [Column("atanan_kullanici_id")]
    public long? AtananKullaniciId { get; set; }

    [Column("durum")]
    public IsEmriDurumu Durum { get; set; } = IsEmriDurumu.ACIK;

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

    [Column("sokulen_seri_no")]
    public string? SokulenSeriNo { get; set; }

    [Column("yeni_seri_no")]
    public string? YeniSeriNo { get; set; }

    [Column("yeni_muhur_no")]
    public string? YeniMuhurNo { get; set; }

    [Column("eski_muhur_no")]
    public string? EskiMuhurNo { get; set; }

    [Column("damga_yili")]
    public int? DamgaYili { get; set; }

    [Column("akim_trafosu_seri_no")]
    public string? AkimTrafosuSeriNo { get; set; }

    [Column("akim_trafosu_marka")]
    public string? AkimTrafosuMarka { get; set; }

    [Column("gerilim_trafosu_seri_no")]
    public string? GerilimTrafosuSeriNo { get; set; }

    [Column("gerilim_trafosu_marka")]
    public string? GerilimTrafosuMarka { get; set; }

    [Column("ariza_tipi")]
    public string? ArizaTipi { get; set; }

    [Column("kesme_noktasi")]
    public string? KesmeNoktasi { get; set; }

    [Column("kesme_nedeni")]
    public string? KesmeNedeni { get; set; }

    [Column("abone_durumu")]
    public string? AboneDurumu { get; set; }

    [Column("sayac_durumu")]
    public string? SayacDurumu { get; set; }

    [Column("pano_direk_no")]
    public string? PanoDirekNo { get; set; }

    [Column("kesif_sonucu")]
    public string? KesifSonucu { get; set; }

    [Column("yapi_tesis_tipi")]
    public string? YapiTesisTipi { get; set; }

    [Column("hat_mesafesi")]
    public string? HatMesafesi { get; set; }

    [Column("talep_gucu")]
    public string? TalepGucu { get; set; }

    [Column("inceleme_notu")]
    public string? IncelemeNotu { get; set; }
    
    [Column("acma_noktasi")]
    public string? AcmaNoktasi { get; set; }

    [JsonIgnore]
    public virtual Kullanicilar? AtananKullanici { get; set; }
    
    [JsonIgnore]
    public virtual ICollection<EndeksOkuma> EndeksOkumas { get; set; } = new List<EndeksOkuma>();

    [JsonIgnore]
    public virtual Sayaclar? Sayac { get; set; }

    [JsonIgnore]
    public virtual TuketimNoktasi TuketimNoktasi { get; set; } = null!;
}

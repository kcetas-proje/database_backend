using System;
using System.Collections.Generic;

namespace KcetasAboneApi.Models;

public partial class TuketimNoktasi
{
    public long TuketimNoktasiId { get; set; }

    public string TekilKod { get; set; } = null!;

    public string MusteriAd { get; set; } = null!;

    public string? MusteriSoyad { get; set; }

    public string? MusteriUnvan { get; set; }

    public string? Tckn { get; set; }

    public string? Vkn { get; set; }

    public string? Telefon { get; set; }

    public string? EPosta { get; set; }

    public string? IletisimTercihi { get; set; }

    public int IlceId { get; set; }

    public string Mahalle { get; set; } = null!;

    public string? BinaNo { get; set; }

    public string? BagimsizBolumNo { get; set; }

    public string AcikAdres { get; set; } = null!;

    public decimal? KoordinatLat { get; set; }

    public decimal? KoordinatLon { get; set; }

    public decimal BaglantiGucuKw { get; set; }

    public string TuketiciGrubu { get; set; } = null!;

    public string BaglantiDurumu { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public long? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public long? UpdatedBy { get; set; }

    public virtual Kullanicilar? CreatedByNavigation { get; set; }

    public virtual Ilce Ilce { get; set; } = null!;

    public virtual ICollection<IsEmirleri> IsEmirleris { get; set; } = new List<IsEmirleri>();

    public virtual ICollection<Sayaclar> Sayaclars { get; set; } = new List<Sayaclar>();

    public virtual ICollection<Sozlesmeler> Sozlesmelers { get; set; } = new List<Sozlesmeler>();

    public virtual Kullanicilar? UpdatedByNavigation { get; set; }
}

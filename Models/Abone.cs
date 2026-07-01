using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace KcetasAboneApi.Models;

public partial class Abone
{
    public long AboneId { get; set; }

    public string AboneNo { get; set; } = null!;

    public string AboneTipi { get; set; } = null!;

    public string? Tckn { get; set; }

    public string? Vkn { get; set; }

    public string? Telefon { get; set; }

    public string? EPosta { get; set; }

    public string? IletisimTercihi { get; set; }

    public string? Status { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public long? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public long? UpdatedBy { get; set; }

    public string? Ad { get; set; }

    public string? Soyad { get; set; }

    public string? Unvan { get; set; }

    public long? KullaniciId { get; set; }

    [JsonIgnore]
    public virtual Kullanicilar? CreatedByNavigation { get; set; }

    public virtual ICollection<Fatura> Faturas { get; set; } = new List<Fatura>();

    public virtual Kullanicilar? Kullanici { get; set; }

    [JsonIgnore]
    public virtual ICollection<Sozlesmeler> Sozlesmelers { get; set; } = new List<Sozlesmeler>();

    public virtual Kullanicilar? UpdatedByNavigation { get; set; }
}

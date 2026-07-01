using System;
using System.Collections.Generic;

namespace KcetasAboneApi.Models;

public partial class Sozlesmeler
{
    public long SozlesmeId { get; set; }

    public string SozlesmeNo { get; set; } = null!;

    public long AboneId { get; set; }

    public long TuketimNoktasiId { get; set; }

    public string SozlesmeTipi { get; set; } = null!;

    public DateOnly BaslangicTarihi { get; set; }

    public DateOnly? BitisTarihi { get; set; }

    public string Statu { get; set; } = null!;

    public string TarifeGrubu { get; set; } = null!;

    public decimal GuvenceBedeli { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public long? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public long? UpdatedBy { get; set; }

    public virtual Abone Abone { get; set; } = null!;

    public virtual Kullanicilar? CreatedByNavigation { get; set; }

    public virtual ICollection<EndeksOkuma> EndeksOkumas { get; set; } = new List<EndeksOkuma>();

    public virtual ICollection<Fatura> Faturas { get; set; } = new List<Fatura>();

    public virtual TuketimNoktasi TuketimNoktasi { get; set; } = null!;

    public virtual Kullanicilar? UpdatedByNavigation { get; set; }
}

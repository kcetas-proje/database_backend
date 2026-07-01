using System;
using System.Collections.Generic;

namespace KcetasAboneApi.Models;

public partial class Sayaclar
{
    public long SayacId { get; set; }

    public string SeriNo { get; set; } = null!;

    public long? TuketimNoktasiId { get; set; }

    public string? Marka { get; set; }

    public string? Model { get; set; }

    public string? Faz { get; set; }

    public decimal Carpan { get; set; }

    public string? MuhurNo { get; set; }

    public string Durum { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public long? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public long? UpdatedBy { get; set; }

    public virtual Kullanicilar? CreatedByNavigation { get; set; }

    public virtual ICollection<EndeksOkuma> EndeksOkumas { get; set; } = new List<EndeksOkuma>();

    public virtual ICollection<IsEmirleri> IsEmirleris { get; set; } = new List<IsEmirleri>();

    public virtual TuketimNoktasi? TuketimNoktasi { get; set; }

    public virtual Kullanicilar? UpdatedByNavigation { get; set; }
}

using System;
using System.Collections.Generic;

namespace KcetasAboneApi.Models;

public partial class Kullanicilar
{
    public long KullaniciId { get; set; }

    public string AdSoyad { get; set; } = null!;

    public string KullaniciAdi { get; set; } = null!;

    public string EPosta { get; set; } = null!;

    public string SifreHash { get; set; } = null!;

    public short RolId { get; set; }

    public string Durum { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<EndeksOkuma> EndeksOkumas { get; set; } = new List<EndeksOkuma>();

    public virtual ICollection<IsEmirleri> IsEmirleris { get; set; } = new List<IsEmirleri>();

    public virtual Roller Rol { get; set; } = null!;

    public virtual ICollection<Sayaclar> SayaclarCreatedByNavigations { get; set; } = new List<Sayaclar>();

    public virtual ICollection<Sayaclar> SayaclarUpdatedByNavigations { get; set; } = new List<Sayaclar>();

    public virtual ICollection<Sozlesmeler> SozlesmelerCreatedByNavigations { get; set; } = new List<Sozlesmeler>();

    public virtual ICollection<Sozlesmeler> SozlesmelerUpdatedByNavigations { get; set; } = new List<Sozlesmeler>();

    public virtual ICollection<TuketimNoktasi> TuketimNoktasiCreatedByNavigations { get; set; } = new List<TuketimNoktasi>();

    public virtual ICollection<TuketimNoktasi> TuketimNoktasiUpdatedByNavigations { get; set; } = new List<TuketimNoktasi>();
}

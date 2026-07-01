using System;
using System.Collections.Generic;

namespace KcetasAboneApi.Models;

public partial class EndeksOkuma
{
    public long OkumaId { get; set; }

    public long SayacId { get; set; }

    public long? IsEmriId { get; set; }

    public long? SozlesmeId { get; set; }

    public string OkumaTipi { get; set; } = null!;

    public string OkumaKaynagi { get; set; } = null!;

    public decimal? OncekiEndeks { get; set; }

    public decimal YeniEndeks { get; set; }

    public string? Donem { get; set; }

    public DateTime OkumaZamani { get; set; }

    public long? KullaniciId { get; set; }

    public string? OkunamamaNedeni { get; set; }

    public string DogrulamaDurumu { get; set; } = null!;

    public bool AnomaliMi { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Fatura> Faturas { get; set; } = new List<Fatura>();

    public virtual IsEmirleri? IsEmri { get; set; }

    public virtual Kullanicilar? Kullanici { get; set; }

    public virtual Sayaclar Sayac { get; set; } = null!;

    public virtual Sozlesmeler? Sozlesme { get; set; }
}

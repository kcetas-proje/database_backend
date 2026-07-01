using System;
using System.Collections.Generic;

namespace KcetasAboneApi.Models;

public partial class FaturaKalemi
{
    public long FaturaKalemId { get; set; }

    public long FaturaId { get; set; }

    public string KalemTipi { get; set; } = null!;

    public string? Aciklama { get; set; }

    public decimal Miktar { get; set; }

    public decimal? BirimFiyat { get; set; }

    public decimal Tutar { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Fatura Fatura { get; set; } = null!;
}

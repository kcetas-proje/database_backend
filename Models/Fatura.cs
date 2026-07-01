using System;
using System.Collections.Generic;

namespace KcetasAboneApi.Models;

public partial class Fatura
{
    public long FaturaId { get; set; }

    public string FaturaNo { get; set; } = null!;

    public long SozlesmeId { get; set; }

    public string TekilKod { get; set; } = null!;

    public long AboneId { get; set; }

    public string FaturaTipi { get; set; } = null!;

    public string Donem { get; set; } = null!;

    public DateOnly FaturaTarihi { get; set; }

    public DateOnly SonOdemeTarihi { get; set; }

    public long? OkumaId { get; set; }

    public decimal? IlkEndeks { get; set; }

    public decimal? SonEndeks { get; set; }

    public decimal? TuketimKwh { get; set; }

    public decimal? ReaktifEnduktif { get; set; }

    public decimal? ReaktifKapasitif { get; set; }

    public decimal? Carpan { get; set; }

    public decimal EnerjiBedeli { get; set; }

    public decimal DagitimBedeli { get; set; }

    public decimal HizmetBedeli { get; set; }

    public decimal KesmeBaglamaBedeli { get; set; }

    public decimal VergiFonToplam { get; set; }

    public decimal ToplamTutar { get; set; }

    public string Durum { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Abone Abone { get; set; } = null!;

    public virtual ICollection<EntegrasyonOutbox> EntegrasyonOutboxes { get; set; } = new List<EntegrasyonOutbox>();

    public virtual ICollection<FaturaKalemi> FaturaKalemis { get; set; } = new List<FaturaKalemi>();

    public virtual EndeksOkuma? Okuma { get; set; }

    public virtual Sozlesmeler Sozlesme { get; set; } = null!;
}

using System;
using System.Collections.Generic;

namespace KcetasAboneApi.Models;

public partial class EntegrasyonOutbox
{
    public long OutboxId { get; set; }

    public long FaturaId { get; set; }

    public HedefSistem HedefSistem { get; set; }

    public string IdempotencyKey { get; set; } = null!;

    public string? CorrelationId { get; set; }

    public string? Payload { get; set; }

    public OutboxDurumu Durum { get; set; }

    public string? HataKodu { get; set; }

    public string? HataMesaji { get; set; }

    public int RetryCount { get; set; }

    public DateTime? SonDenemeTarihi { get; set; }

    public DateTime? GonderimZamani { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Fatura Fatura { get; set; } = null!;
}

using System;
using System.Collections.Generic;

namespace KcetasAboneApi.Models;

public partial class AuditLog
{
    public long AuditId { get; set; }

    public string VarlikTipi { get; set; } = null!;

    public long VarlikId { get; set; }

    public string IslemTipi { get; set; } = null!;

    public string? EskiDeger { get; set; }

    public string? YeniDeger { get; set; }

    public long? KullaniciId { get; set; }

    public string? IslemGerekcesi { get; set; }

    public DateTime IslemZamani { get; set; }

    public virtual Kullanicilar? Kullanici { get; set; }
}

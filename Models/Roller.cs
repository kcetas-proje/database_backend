using System;
using System.Collections.Generic;

namespace KcetasAboneApi.Models;

public partial class Roller
{
    public short RolId { get; set; }

    public string RolAdi { get; set; } = null!;

    public string? Aciklama { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Kullanicilar> Kullanicilars { get; set; } = new List<Kullanicilar>();
}

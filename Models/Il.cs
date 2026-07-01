using System;
using System.Collections.Generic;

namespace KcetasAboneApi.Models;

public partial class Il
{
    public short IlId { get; set; }

    public string IlAdi { get; set; } = null!;

    public short? PlakaKodu { get; set; }

    public virtual ICollection<Ilce> Ilces { get; set; } = new List<Ilce>();
}

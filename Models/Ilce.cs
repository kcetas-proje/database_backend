using System;
using System.Collections.Generic;

namespace KcetasAboneApi.Models;

public partial class Ilce
{
    public int IlceId { get; set; }

    public short IlId { get; set; }

    public string IlceAdi { get; set; } = null!;

    public virtual Il Il { get; set; } = null!;

    public virtual ICollection<TuketimNoktasi> TuketimNoktasis { get; set; } = new List<TuketimNoktasi>();
}

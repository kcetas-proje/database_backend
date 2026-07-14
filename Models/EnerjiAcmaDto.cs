namespace KcetasAboneApi.Models;

public class EnerjiAcmaDto
{
    public long IsEmriId { get; set; }

    public string AcmaNoktasi { get; set; } = "";

    public decimal Aktif { get; set; }

    public decimal Gunduz { get; set; }

    public decimal Puant { get; set; }

    public decimal Gece { get; set; }

    public decimal Induktif { get; set; }

    public decimal Kapasitif { get; set; }

    public decimal Demand { get; set; }

    public string MuhurNo { get; set; } = "";

    public string Aciklama { get; set; } = "";
}

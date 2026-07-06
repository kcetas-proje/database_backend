namespace KcetasAboneApi.Models;
public class FaturaCreateDto
    {
        public long SozlesmeId { get; set; }
        public long? OkumaId { get; set; }
        public decimal TuketimKwh { get; set; }
        public decimal ToplamTutar { get; set; }
        public string? Donem { get; set; }
    }
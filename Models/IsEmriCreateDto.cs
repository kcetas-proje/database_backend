namespace KcetasAboneApi.Models;

public class IsEmriCreateDto
    {
        public long TuketimNoktasiId { get; set; }
        public long? SayacId { get; set; }
        public string Tip { get; set; } = null!;
        public string? Oncelik { get; set; }
        public DateTime? PlanlananTarih { get; set; }
        public long? AtananKullaniciId { get; set; }
    }

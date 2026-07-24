using KcetasAboneApi.Models;

namespace KcetasAboneApi.DTOs.IsEmirleri
{
    public class IsEmriListDto
    {
        public long IsEmriId { get; set; }
        public string IsEmriNo { get; set; } = string.Empty;

        public IsEmriTipi Tip { get; set; }
        public IsEmriDurumu Durum { get; set; }

        public long TuketimNoktasiId { get; set; }
        public long? SayacId { get; set; }
        public long? AtananKullaniciId { get; set; }

        public DateTime? PlanlananTarih { get; set; }
        public DateTime CreatedAt { get; set; }

        // Frontend için eklenen alanlar
        public string? SozlesmeNo { get; set; }
        public string? SayacSeriNo { get; set; }
        public string? AboneAdi { get; set; }
        public string? AboneNo { get; set; }
        public string? Adres { get; set; }
    }
}

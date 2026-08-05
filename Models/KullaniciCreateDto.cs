namespace KcetasAboneApi.Models;

public class KullaniciCreateDto
    {
        public string AdSoyad { get; set; } = null!;
        public string KullaniciAdi { get; set; } = null!;
        public string EPosta { get; set; } = null!;
        public string Sifre { get; set; } = null!;
        public short RolId { get; set; }
    }
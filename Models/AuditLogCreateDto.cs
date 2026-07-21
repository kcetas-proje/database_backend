namespace KcetasAboneApi.Models;

public class AuditLogCreateDto
    {
        public string VarlikTipi { get; set; } = null!;
        public long VarlikId { get; set; }
        public IslemTipi IslemTipi { get; set; }
        public string? EskiDeger { get; set; }
        public string? YeniDeger { get; set; }
        public long? KullaniciId { get; set; }
        public string? IslemGerekcesi { get; set; }
    }
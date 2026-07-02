namespace KcetasAboneApi.Models;

public class EntegrasyonOutboxCreateDto
    {
        public long FaturaId { get; set; }
        public string HedefSistem { get; set; } = null!;
        public string? Payload { get; set; }
    }
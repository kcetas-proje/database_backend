namespace KcetasAboneApi.Models;

public class EntegrasyonOutboxCreateDto
    {
        public long FaturaId { get; set; }
        public HedefSistem HedefSistem { get; set; }
        public string? Payload { get; set; }
    }
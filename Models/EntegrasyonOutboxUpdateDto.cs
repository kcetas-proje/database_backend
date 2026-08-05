namespace KcetasAboneApi.Models
{
    public class EntegrasyonOutboxUpdateDto
{
    public int OutboxId { get; set; }
    public string? Durum { get; set; }
    public int RetryCount { get; set; }
    public string? HataMesaji { get; set; }
}


}
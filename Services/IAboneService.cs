using KcetasAboneApi.Models;

public interface IAboneService
{
    Task<AboneFaturaResponseDto> GetFaturalar(
        string? isim,
        int page,
        int pageSize);
}

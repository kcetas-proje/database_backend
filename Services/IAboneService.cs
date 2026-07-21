using KcetasAboneApi.Models;

public interface IAboneService
{
    Task<AboneFaturaResponseDto> GetFaturalar(
        string? ad,
        int page,
        int pageSize);
}

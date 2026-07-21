using KcetasAboneApi.Models;

public interface IAboneService
{
    Task<AboneListResponseDto> GetAboneler(
        string? isim,
        int page,
        int pageSize);
}

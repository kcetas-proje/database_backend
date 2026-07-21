using KcetasAboneApi.Models;

public interface IAboneService
{
    Task<AboneListResponseDto> GetAboneler(
        string? isim,
        int page,
        int pageSize);

    Task<AboneFaturaResponseDto> GetAboneFaturalari(
        long aboneId,
        int page,
        int pageSize);
}

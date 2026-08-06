using KcetasAboneApi.Models;

public interface IAboneService
{
    /// <summary>
    /// Aboneleri isme göre filtreleyerek sayfalanmış (paged) olarak getirir.
    /// </summary>
    Task<AboneListResponseDto> GetAboneler(
        string? isim,
        int page,
        int pageSize);

    /// <summary>
    /// Belirli bir aboneye ait faturaları sayfalanmış olarak getirir.
    /// </summary>
    Task<AboneFaturaResponseDto> GetAboneFaturalari(
        long aboneId,
        int page,
        int pageSize);
}

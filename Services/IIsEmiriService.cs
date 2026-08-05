namespace KcetasAboneApi.Services;

public interface IIsEmiriService
{
    Task<(bool Success, string Message, int ProcessedCount)> IsEmriTamamlaAsync(long isEmriId, long sayacId);
    Task<(bool Success, string Message, int ProcessedCount)> TopluYeniBaglantiOnaylaAsync();
}

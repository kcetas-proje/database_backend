namespace KcetasAboneApi.Services;

public interface IIsEmiriService
{
    /// <summary>
    /// Belirtilen açık iş emrini tamamlar ve verilen sayacı o noktaya takılı duruma getirir.
    /// </summary>
    Task<(bool Success, string Message, int ProcessedCount)> IsEmriTamamlaAsync(long isEmriId, long sayacId);
    
    /// <summary>
    /// Sistemde bekleyen tüm "YENİ BAĞLANTI" iş emirlerini bulur, depodaki uygun sayaçları tahsis ederek iş emirlerini otomatik olarak tamamlar.
    /// </summary>
    Task<(bool Success, string Message, int ProcessedCount)> TopluYeniBaglantiOnaylaAsync();
}

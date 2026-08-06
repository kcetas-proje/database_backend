using KcetasAboneApi.Models;

namespace KcetasAboneApi.Services;

public interface IFaturaService
{
    /// <summary>
    /// Sistemde onaylanmış olan tüm endeks okumalarını tarayarak toplu fatura kesim işlemi yapar.
    /// </summary>
    Task<(int FaturaSayisi, decimal ToplamTutar)> OnaylanmisEndeksleriFaturalandirAsync();
}

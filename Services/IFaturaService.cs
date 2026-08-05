using KcetasAboneApi.Models;

namespace KcetasAboneApi.Services;

public interface IFaturaService
{
    Task<(int FaturaSayisi, decimal ToplamTutar)> OnaylanmisEndeksleriFaturalandirAsync();
}

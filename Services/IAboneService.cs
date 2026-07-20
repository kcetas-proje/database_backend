using KcetasAboneApi.Models;

public interface IAboneService
{
    Task<List<AboneFaturaDto>> GetSon10Fatura(string ilkUcHane);
}

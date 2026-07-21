namespace KcetasAboneApi.Models;

public class AboneFaturaResponseDto
{
    public int TotalCount { get; set; }

    public int CurrentPage { get; set; }

    public int PageSize { get; set; }

    public bool HasNextPage { get; set; }

    public List<AboneFaturaDto> Data { get; set; } = new();
}

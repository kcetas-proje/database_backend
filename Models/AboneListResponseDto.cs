namespace KcetasAboneApi.Models;

public class AboneListResponseDto
{
    public int TotalCount { get; set; }

    public int CurrentPage { get; set; }

    public int PageSize { get; set; }

    public bool HasNextPage { get; set; }

    public List<AboneResponseDto> Data { get; set; } = new();
}

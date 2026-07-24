using System.Collections.Generic;

namespace KcetasAboneApi.Models;

public class PagedResultDto<T>
{
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public long? NextCursor { get; set; }
    public IEnumerable<T> Data { get; set; } = new List<T>();
}

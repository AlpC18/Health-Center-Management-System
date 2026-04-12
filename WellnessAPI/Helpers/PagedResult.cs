namespace WellnessAPI.Helpers;

public class PagedResult<T>
{
    public List<T> Data { get; set; } = new();
    public int Page { get; set; }
    public int Limit { get; set; }
    public int Total { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)Total / Limit);
    public bool HasNext => Page < TotalPages;
    public bool HasPrev => Page > 1;
}

public class PagedQuery
{
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 10;
    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public string SortDir { get; set; } = "asc";
}

namespace WellnessAPI.Helpers
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();
        public int Total { get; set; }
        public int Page { get; set; }
        public int Limit { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)Total / Limit);
    }
}

namespace NuvTools.Data.Paging;

public abstract class PagingBase<T, R> where T : IEnumerable<R>
{
    public required int PageNumber { get; set; }

    public required int Total { get; set; }

    public required T List { get; set; }
}

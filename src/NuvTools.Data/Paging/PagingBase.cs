namespace NuvTools.Data.Paging;

public abstract record PagingBase<T, R> where T : IEnumerable<R>
{
    public int PageNumber { get; set; }

    public int Total { get; set; }

    public T List { get; set; }
}

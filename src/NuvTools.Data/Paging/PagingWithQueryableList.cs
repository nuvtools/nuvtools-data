namespace NuvTools.Data.Paging;

public record PagingWithQueryableList<T> : PagingBase<IQueryable<T>, T>
{
}

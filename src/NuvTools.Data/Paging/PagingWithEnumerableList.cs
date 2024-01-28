namespace NuvTools.Data.Paging;

public record PagingWithEnumerableList<T> : PagingBase<IEnumerable<T>, T>
{
}
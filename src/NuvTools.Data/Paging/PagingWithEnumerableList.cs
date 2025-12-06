namespace NuvTools.Data.Paging;

/// <summary>
/// Represents a paged result with an IEnumerable list for in-memory collections.
/// </summary>
/// <typeparam name="T">The type of items in the enumerable collection.</typeparam>
public class PagingWithEnumerableList<T> : PagingBase<IEnumerable<T>, T>
{
}
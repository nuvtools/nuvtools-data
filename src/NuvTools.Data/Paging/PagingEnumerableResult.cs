namespace NuvTools.Data.Paging;

/// <summary>
/// Represents a paged result with an IEnumerable list for in-memory collections,
/// supporting optimized count modes for large datasets.
/// </summary>
/// <typeparam name="T">The type of items in the enumerable collection.</typeparam>
public class PagingEnumerableResult<T> : PagingResult<IEnumerable<T>, T>
{
}

namespace NuvTools.Data.Paging;

/// <summary>
/// Represents a paged result with an IQueryable list for deferred execution scenarios,
/// supporting optimized count modes for large datasets.
/// Typically used with Entity Framework Core for database queries.
/// </summary>
/// <typeparam name="T">The type of items in the queryable collection.</typeparam>
public class PagingQueryableResult<T> : PagingResult<IQueryable<T>, T>
{
}

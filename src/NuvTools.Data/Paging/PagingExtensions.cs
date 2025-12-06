namespace NuvTools.Data.Paging;

/// <summary>
/// Extension methods for paging collections.
/// </summary>
public static class PagingExtensions
{

    /// <summary>
    /// Wraps an enumerable collection into a paged result with metadata.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="list">The source enumerable collection.</param>
    /// <param name="pageNumber">The page number (1-indexed). Defaults to 1.</param>
    /// <param name="pageSize">The number of items per page. Defaults to 50.</param>
    /// <returns>A paged result containing the requested page of data with total count.</returns>
    public static PagingWithEnumerableList<T> PagingWrap<T>(this IEnumerable<T> list, int pageNumber = 1, int pageSize = 50)
    {
        var total = list.Count();
        return new PagingWithEnumerableList<T>
        {
            List = list.Paging(pageNumber, pageSize),
            PageNumber = PagingHelper.GetPageNumber(pageNumber, pageSize, total),
            Total = total
        };
    }

    /// <summary>
    /// Applies paging to an IQueryable collection by skipping and taking the appropriate number of items.
    /// </summary>
    /// <typeparam name="T">The type of items in the queryable collection.</typeparam>
    /// <param name="list">The source queryable collection.</param>
    /// <param name="pageNumber">The page number (1-indexed). Defaults to 1.</param>
    /// <param name="pageSize">The number of items per page. Defaults to 50.</param>
    /// <returns>A queryable collection containing only the items for the requested page.</returns>
    public static IQueryable<T> Paging<T>(this IQueryable<T> list, int pageNumber = 1, int pageSize = 50) => list.Skip(PagingHelper.GetSkip(pageNumber, pageSize)).Take(pageSize);

    /// <summary>
    /// Applies paging to an IEnumerable collection by skipping and taking the appropriate number of items.
    /// </summary>
    /// <typeparam name="T">The type of items in the enumerable collection.</typeparam>
    /// <param name="list">The source enumerable collection.</param>
    /// <param name="pageNumber">The page number (1-indexed). Defaults to 1.</param>
    /// <param name="pageSize">The number of items per page. Defaults to 50.</param>
    /// <returns>An enumerable collection containing only the items for the requested page.</returns>
    public static IEnumerable<T> Paging<T>(this IEnumerable<T> list, int pageNumber = 1, int pageSize = 50) => list.Skip(PagingHelper.GetSkip(pageNumber, pageSize)).Take(pageSize);

    #region Conversions

    /// <summary>
    /// Converts a PagingWithQueryableList to a PagingWithEnumerableList by materializing the queryable collection.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="paging">The source paging result with queryable list.</param>
    /// <returns>A paging result with an enumerable list.</returns>
    /// <exception cref="ArgumentNullException">Thrown when paging is null.</exception>
    public static PagingWithEnumerableList<T> ToPagingEnumerable<T>(this PagingWithQueryableList<T> paging)
    {
        ArgumentNullException.ThrowIfNull(paging);
        return new PagingWithEnumerableList<T>
        {
            List = [.. paging.List],
            PageNumber = paging.PageNumber,
            Total = paging.Total
        };
    }

    #endregion

}
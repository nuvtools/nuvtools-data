using NuvTools.Data.Paging.Enumerations;

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
    /// Wraps an enumerable collection into a paged result with configurable count options.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="list">The source enumerable collection.</param>
    /// <param name="pageNumber">The page number (1-indexed).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="options">The paging options controlling count behavior.</param>
    /// <returns>A paged result with optional total count based on the options.</returns>
    public static PagingEnumerableResult<T> PagingWrap<T>(this IEnumerable<T> list, int pageNumber, int pageSize, PagingOptions options)
    {
        ArgumentNullException.ThrowIfNull(list);
        ArgumentNullException.ThrowIfNull(options);

        // If pre-calculated total is provided, use it
        if (options.PreCalculatedTotal.HasValue)
        {
            return new PagingEnumerableResult<T>
            {
                List = list.Paging(pageNumber, pageSize),
                PageNumber = PagingHelper.GetPageNumber(pageNumber, pageSize, options.PreCalculatedTotal.Value),
                Total = options.PreCalculatedTotal.Value
            };
        }

        return options.CountMode switch
        {
            CountMode.Skip => PagingWrapSkipCount(list, pageNumber, pageSize),
            CountMode.HasMore => PagingWrapWithHasMore(list, pageNumber, pageSize),
            CountMode.Threshold => PagingWrapWithThreshold(list, pageNumber, pageSize, options.CountThreshold),
            _ => PagingWrapWithCount(list, pageNumber, pageSize) // CountMode.Always
        };
    }

    /// <summary>
    /// Wraps an enumerable collection into a paged result with a pre-calculated total, skipping the count operation.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="list">The source enumerable collection.</param>
    /// <param name="total">The pre-calculated total count.</param>
    /// <param name="pageNumber">The page number (1-indexed). Defaults to 1.</param>
    /// <param name="pageSize">The number of items per page. Defaults to 50.</param>
    /// <returns>A paged result using the provided total.</returns>
    public static PagingEnumerableResult<T> PagingWrapWithTotal<T>(this IEnumerable<T> list, int total, int pageNumber = 1, int pageSize = 50)
    {
        ArgumentNullException.ThrowIfNull(list);

        return new PagingEnumerableResult<T>
        {
            List = list.Paging(pageNumber, pageSize),
            PageNumber = PagingHelper.GetPageNumber(pageNumber, pageSize, total),
            Total = total
        };
    }

    /// <summary>
    /// Wraps an enumerable collection into a paged result without executing a count operation.
    /// Use this for infinite scroll or "load more" patterns.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="list">The source enumerable collection.</param>
    /// <param name="pageNumber">The page number (1-indexed). Defaults to 1.</param>
    /// <param name="pageSize">The number of items per page. Defaults to 50.</param>
    /// <returns>A paged result with Total set to null.</returns>
    public static PagingEnumerableResult<T> PagingWrapSkipCount<T>(this IEnumerable<T> list, int pageNumber = 1, int pageSize = 50)
    {
        ArgumentNullException.ThrowIfNull(list);

        return new PagingEnumerableResult<T>
        {
            List = list.Paging(pageNumber, pageSize),
            PageNumber = PagingHelper.GetPageNumberWithoutTotal(pageNumber),
            Total = null
        };
    }

    /// <summary>
    /// Wraps an enumerable collection into a paged result using the HasMore pattern (fetch N+1).
    /// Determines if more records exist without executing a count query.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="list">The source enumerable collection.</param>
    /// <param name="pageNumber">The page number (1-indexed). Defaults to 1.</param>
    /// <param name="pageSize">The number of items per page. Defaults to 50.</param>
    /// <returns>A paged result with HasMore indicator instead of total count.</returns>
    public static PagingEnumerableResult<T> PagingWrapWithHasMore<T>(this IEnumerable<T> list, int pageNumber = 1, int pageSize = 50)
    {
        ArgumentNullException.ThrowIfNull(list);

        // Fetch pageSize + 1 to determine if there are more records
        var skip = PagingHelper.GetSkip(pageNumber, pageSize);
        var items = list.Skip(skip).Take(pageSize + 1).ToList();

        var hasMore = items.Count > pageSize;
        var resultItems = hasMore ? items.Take(pageSize) : items;

        return new PagingEnumerableResult<T>
        {
            List = resultItems,
            PageNumber = PagingHelper.GetPageNumberWithoutTotal(pageNumber),
            Total = null,
            HasMore = hasMore
        };
    }

    /// <summary>
    /// Wraps an enumerable collection into a paged result, counting only up to a threshold.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="list">The source enumerable collection.</param>
    /// <param name="pageNumber">The page number (1-indexed).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="threshold">The maximum count threshold.</param>
    /// <returns>A paged result with total capped at the threshold.</returns>
    public static PagingEnumerableResult<T> PagingWrapWithThreshold<T>(this IEnumerable<T> list, int pageNumber, int pageSize, int threshold)
    {
        ArgumentNullException.ThrowIfNull(list);

        // Count only up to threshold + 1 to know if we exceeded
        var countUpToThreshold = list.Take(threshold + 1).Count();
        var total = Math.Min(countUpToThreshold, threshold);

        return new PagingEnumerableResult<T>
        {
            List = list.Paging(pageNumber, pageSize),
            PageNumber = PagingHelper.GetPageNumber(pageNumber, pageSize, total),
            Total = total,
            HasMore = countUpToThreshold > threshold ? true : null
        };
    }

    /// <summary>
    /// Internal method to wrap with full count (used by options-based overload).
    /// </summary>
    private static PagingEnumerableResult<T> PagingWrapWithCount<T>(IEnumerable<T> list, int pageNumber, int pageSize)
    {
        var total = list.Count();
        return new PagingEnumerableResult<T>
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

    /// <summary>
    /// Converts a PagingQueryableResult to a PagingEnumerableResult by materializing the queryable collection.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="paging">The source paging result with queryable list.</param>
    /// <returns>A paging result with an enumerable list.</returns>
    /// <exception cref="ArgumentNullException">Thrown when paging is null.</exception>
    public static PagingEnumerableResult<T> ToPagingEnumerable<T>(this PagingQueryableResult<T> paging)
    {
        ArgumentNullException.ThrowIfNull(paging);
        return new PagingEnumerableResult<T>
        {
            List = [.. paging.List],
            PageNumber = paging.PageNumber,
            Total = paging.Total,
            HasMore = paging.HasMore
        };
    }

    #endregion

}
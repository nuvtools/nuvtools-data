using Microsoft.EntityFrameworkCore;
using NuvTools.Data.Paging;
using NuvTools.Data.Paging.Enumerations;

namespace NuvTools.Data.EntityFrameworkCore.Paging;

/// <summary>
/// Extension methods for asynchronous paging of Entity Framework Core queryable collections.
/// </summary>
public static class PagingExtensions
{
    /// <summary>
    /// Wraps a queryable collection into a paged result with metadata, using asynchronous database operations.
    /// </summary>
    /// <typeparam name="T">The type of items in the queryable collection.</typeparam>
    /// <param name="list">The source queryable collection.</param>
    /// <param name="pageNumber">The page number (1-indexed). Defaults to 1.</param>
    /// <param name="pageSize">The number of items per page. Defaults to 30.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation, containing a paged result with queryable list.</returns>
    /// <exception cref="ArgumentNullException">Thrown when list is null.</exception>
    public static async Task<PagingWithQueryableList<T>> PagingWrapAsync<T>(this IQueryable<T> list, int pageNumber = 1, int pageSize = 30, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(list, nameof(list));

        var total = await list.CountAsync(cancellationToken).ConfigureAwait(false);

        return new PagingWithQueryableList<T>
        {
            List = list.Paging(pageNumber, pageSize),
            PageNumber = PagingHelper.GetPageNumber(pageNumber, pageSize, total),
            Total = total
        };
    }

    /// <summary>
    /// Wraps a queryable collection into a paged result with configurable count options.
    /// Use this overload to optimize large dataset paging by skipping or limiting count queries.
    /// </summary>
    /// <typeparam name="T">The type of items in the queryable collection.</typeparam>
    /// <param name="list">The source queryable collection.</param>
    /// <param name="pageNumber">The page number (1-indexed).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="options">The paging options controlling count behavior.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation, containing a paged result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when list or options is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when CountMode.Approximate is used without ApproximateCountProvider.</exception>
    public static async Task<PagingQueryableResult<T>> PagingWrapAsync<T>(this IQueryable<T> list, int pageNumber, int pageSize, PagingOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(list, nameof(list));
        ArgumentNullException.ThrowIfNull(options, nameof(options));

        // If pre-calculated total is provided, use it
        if (options.PreCalculatedTotal.HasValue)
        {
            return new PagingQueryableResult<T>
            {
                List = list.Paging(pageNumber, pageSize),
                PageNumber = PagingHelper.GetPageNumber(pageNumber, pageSize, options.PreCalculatedTotal.Value),
                Total = options.PreCalculatedTotal.Value
            };
        }

        return options.CountMode switch
        {
            CountMode.Skip => PagingWrapSkipCountAsync(list, pageNumber, pageSize),
            CountMode.HasMore => await PagingWrapWithHasMoreAsync(list, pageNumber, pageSize, cancellationToken).ConfigureAwait(false),
            CountMode.Threshold => await PagingWrapWithThresholdAsync(list, pageNumber, pageSize, options.CountThreshold, cancellationToken).ConfigureAwait(false),
            CountMode.Approximate => await PagingWrapWithApproximateAsync(list, pageNumber, pageSize, options, cancellationToken).ConfigureAwait(false),
            _ => await PagingWrapWithCountAsync(list, pageNumber, pageSize, cancellationToken).ConfigureAwait(false) // CountMode.Always
        };
    }

    /// <summary>
    /// Wraps a queryable into a paged result with DbContext support for approximate count.
    /// When CountMode.Approximate is used, the ApproximateCountProvider delegate in options is invoked.
    /// </summary>
    /// <typeparam name="TEntity">The entity type being queried.</typeparam>
    /// <param name="list">The source queryable collection.</param>
    /// <param name="context">The DbContext instance (available for use by database-specific extensions).</param>
    /// <param name="pageNumber">The page number (1-indexed).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="options">The paging options controlling count behavior.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation, containing a paged result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when list, context, or options is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when CountMode.Approximate is used without ApproximateCountProvider.</exception>
    public static async Task<PagingQueryableResult<TEntity>> PagingWrapAsync<TEntity>(
        this IQueryable<TEntity> list,
        DbContext context,
        int pageNumber,
        int pageSize,
        PagingOptions options,
        CancellationToken cancellationToken = default) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(list);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(options);

        return await list.PagingWrapAsync(pageNumber, pageSize, options, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Wraps a queryable collection into a paged result with a pre-calculated total, skipping the count query.
    /// Use this when the total is cached or already known from a previous operation.
    /// </summary>
    /// <typeparam name="T">The type of items in the queryable collection.</typeparam>
    /// <param name="list">The source queryable collection.</param>
    /// <param name="total">The pre-calculated total count.</param>
    /// <param name="pageNumber">The page number (1-indexed). Defaults to 1.</param>
    /// <param name="pageSize">The number of items per page. Defaults to 30.</param>
    /// <returns>A paged result using the provided total.</returns>
    /// <exception cref="ArgumentNullException">Thrown when list is null.</exception>
    public static PagingQueryableResult<T> PagingWrapWithTotalAsync<T>(this IQueryable<T> list, int total, int pageNumber = 1, int pageSize = 30)
    {
        ArgumentNullException.ThrowIfNull(list, nameof(list));

        return new PagingQueryableResult<T>
        {
            List = list.Paging(pageNumber, pageSize),
            PageNumber = PagingHelper.GetPageNumber(pageNumber, pageSize, total),
            Total = total
        };
    }

    /// <summary>
    /// Wraps a queryable collection into a paged result without executing a count query.
    /// Use this for infinite scroll or "load more" patterns where the total is not needed.
    /// </summary>
    /// <typeparam name="T">The type of items in the queryable collection.</typeparam>
    /// <param name="list">The source queryable collection.</param>
    /// <param name="pageNumber">The page number (1-indexed). Defaults to 1.</param>
    /// <param name="pageSize">The number of items per page. Defaults to 30.</param>
    /// <returns>A paged result with Total set to null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when list is null.</exception>
    public static PagingQueryableResult<T> PagingWrapSkipCountAsync<T>(this IQueryable<T> list, int pageNumber = 1, int pageSize = 30)
    {
        ArgumentNullException.ThrowIfNull(list, nameof(list));

        return new PagingQueryableResult<T>
        {
            List = list.Paging(pageNumber, pageSize),
            PageNumber = PagingHelper.GetPageNumberWithoutTotal(pageNumber),
            Total = null
        };
    }

    /// <summary>
    /// Wraps a queryable collection into a paged result using the HasMore pattern (fetch N+1).
    /// Determines if more records exist without executing a count query.
    /// </summary>
    /// <typeparam name="T">The type of items in the queryable collection.</typeparam>
    /// <param name="list">The source queryable collection.</param>
    /// <param name="pageNumber">The page number (1-indexed). Defaults to 1.</param>
    /// <param name="pageSize">The number of items per page. Defaults to 30.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation, containing a paged result with HasMore indicator.</returns>
    /// <exception cref="ArgumentNullException">Thrown when list is null.</exception>
    public static async Task<PagingQueryableResult<T>> PagingWrapWithHasMoreAsync<T>(this IQueryable<T> list, int pageNumber = 1, int pageSize = 30, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(list, nameof(list));

        // Skip to the current page position and fetch pageSize + 1 to determine if there are more records
        var skip = PagingHelper.GetSkip(pageNumber, pageSize);
        var items = await list.Skip(skip).Take(pageSize + 1).ToListAsync(cancellationToken).ConfigureAwait(false);

        var hasMore = items.Count > pageSize;
        var resultItems = hasMore ? items.Take(pageSize).AsQueryable() : items.AsQueryable();

        return new PagingQueryableResult<T>
        {
            List = resultItems,
            PageNumber = PagingHelper.GetPageNumberWithoutTotal(pageNumber),
            Total = null,
            HasMore = hasMore
        };
    }

    /// <summary>
    /// Wraps a queryable collection into a paged result, counting only up to a threshold.
    /// Use this for large datasets where an exact count is not needed (e.g., "10,000+ results").
    /// </summary>
    /// <typeparam name="T">The type of items in the queryable collection.</typeparam>
    /// <param name="list">The source queryable collection.</param>
    /// <param name="pageNumber">The page number (1-indexed).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="threshold">The maximum count threshold.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation, containing a paged result with total capped at threshold.</returns>
    /// <exception cref="ArgumentNullException">Thrown when list is null.</exception>
    public static async Task<PagingQueryableResult<T>> PagingWrapWithThresholdAsync<T>(this IQueryable<T> list, int pageNumber, int pageSize, int threshold, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(list, nameof(list));

        // Count only up to threshold + 1 to know if we exceeded
        var countUpToThreshold = await list.Take(threshold + 1).CountAsync(cancellationToken).ConfigureAwait(false);
        var total = Math.Min(countUpToThreshold, threshold);

        return new PagingQueryableResult<T>
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
    private static async Task<PagingQueryableResult<T>> PagingWrapWithCountAsync<T>(IQueryable<T> list, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var total = await list.CountAsync(cancellationToken).ConfigureAwait(false);
        return new PagingQueryableResult<T>
        {
            List = list.Paging(pageNumber, pageSize),
            PageNumber = PagingHelper.GetPageNumber(pageNumber, pageSize, total),
            Total = total
        };
    }

    /// <summary>
    /// Internal method to wrap with approximate count using the provider delegate.
    /// </summary>
    private static async Task<PagingQueryableResult<T>> PagingWrapWithApproximateAsync<T>(IQueryable<T> list, int pageNumber, int pageSize, PagingOptions options, CancellationToken cancellationToken)
    {
        if (options.ApproximateCountProvider == null)
            throw new InvalidOperationException(
                "CountMode.Approximate requires ApproximateCountProvider to be set. " +
                "Use PagingOptions.WithApproximateCount() or database-specific extensions.");

        var approxCount = await options.ApproximateCountProvider(cancellationToken).ConfigureAwait(false);
        var total = (int)approxCount;

        return new PagingQueryableResult<T>
        {
            List = list.Paging(pageNumber, pageSize),
            PageNumber = PagingHelper.GetPageNumber(pageNumber, pageSize, total),
            Total = total
        };
    }

    /// <summary>
    /// Wraps a queryable collection into a paged result with an enumerable list, materializing the data asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of items in the queryable collection.</typeparam>
    /// <param name="list">The source queryable collection.</param>
    /// <param name="pageNumber">The page number (1-indexed). Defaults to 1.</param>
    /// <param name="pageSize">The number of items per page. Defaults to 30.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation, containing a paged result with enumerable list.</returns>
    public static async Task<PagingWithEnumerableList<T>> PagingWrapWithEnumerableListAsync<T>(this IQueryable<T> list, int pageNumber = 1, int pageSize = 30, CancellationToken cancellationToken = default)
    {
        var pagedQueryable = await list.PagingWrapAsync(pageNumber, pageSize, cancellationToken);
        return await pagedQueryable.ToPagingWithEnumerableListAsync(cancellationToken);
    }

    #region Conversions

    /// <summary>
    /// Converts a PagingWithQueryableList to a PagingWithEnumerableList by materializing the queryable asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="paging">The source paging result with queryable list.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation, containing a paging result with enumerable list.</returns>
    /// <exception cref="ArgumentNullException">Thrown when paging is null.</exception>
    public static async Task<PagingWithEnumerableList<T>> ToPagingWithEnumerableListAsync<T>(this PagingWithQueryableList<T> paging, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(paging, nameof(paging));

        var list = await paging.List.ToListAsync(cancellationToken).ConfigureAwait(false);

        return new PagingWithEnumerableList<T>
        {
            List = list,
            PageNumber = paging.PageNumber,
            Total = paging.Total
        };
    }

    /// <summary>
    /// Converts a PagingQueryableResult to a PagingEnumerableResult by materializing the queryable asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="paging">The source paging result with queryable list.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation, containing a paging result with enumerable list.</returns>
    /// <exception cref="ArgumentNullException">Thrown when paging is null.</exception>
    public static async Task<PagingEnumerableResult<T>> ToPagingEnumerableResultAsync<T>(this PagingQueryableResult<T> paging, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(paging, nameof(paging));

        var list = await paging.List.ToListAsync(cancellationToken).ConfigureAwait(false);

        return new PagingEnumerableResult<T>
        {
            List = list,
            PageNumber = paging.PageNumber,
            Total = paging.Total,
            HasMore = paging.HasMore
        };
    }

    #endregion


}
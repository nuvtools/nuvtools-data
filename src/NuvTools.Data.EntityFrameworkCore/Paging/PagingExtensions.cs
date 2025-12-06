using Microsoft.EntityFrameworkCore;
using NuvTools.Data.Paging;

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

    #endregion


}
using Microsoft.EntityFrameworkCore;
using NuvTools.Data.Paging;

namespace NuvTools.Data.EntityFrameworkCore.Paging;

public static class PagingExtensions
{
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

    #region Conversions

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
namespace NuvTools.Data.Paging;

public static class PagingExtensions
{

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

    public static IQueryable<T> Paging<T>(this IQueryable<T> list, int pageNumber = 1, int pageSize = 50) => list.Skip(PagingHelper.GetSkip(pageNumber, pageSize)).Take(pageSize);

    public static IEnumerable<T> Paging<T>(this IEnumerable<T> list, int pageNumber = 1, int pageSize = 50) => list.Skip(PagingHelper.GetSkip(pageNumber, pageSize)).Take(pageSize);

    #region Conversions

    public static PagingWithEnumerableList<T> ToPagingEnumerable<T>(this PagingWithQueryableList<T> paging)
    {
        if (paging is null) throw new ArgumentNullException(nameof(paging));
        return new PagingWithEnumerableList<T>
        {
            List = paging.List.ToList(),
            PageNumber = paging.PageNumber,
            Total = paging.Total
        };
    }

    #endregion

}
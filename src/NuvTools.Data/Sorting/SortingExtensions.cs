using System.Linq.Expressions;

namespace NuvTools.Data.Sorting;

public static class SortingExtensions
{

    public enum SortDirection
    {
        ASC = 0,
        DESC = 1
    }

    public static IOrderedQueryable<T> Sort<T, TKey>(this IQueryable<T> list, Expression<Func<T, TKey>> property, SortDirection direction = SortDirection.ASC)
    {
        if (direction == SortDirection.ASC)
            return list.OrderBy(property);
        else
            return list.OrderByDescending(property);
    }

    public static IOrderedQueryable<T> ThenSort<T, TKey>(this IOrderedQueryable<T> list, Expression<Func<T, TKey>> property, SortDirection direction = SortDirection.ASC)
    {
        if (direction == SortDirection.ASC)
            return list.ThenBy(property);
        else
            return list.ThenByDescending(property);
    }

    public static IOrderedEnumerable<T> Sort<T, TKey>(this IEnumerable<T> list, Func<T, TKey> property, SortDirection direction = SortDirection.ASC)
    {
        if (direction == SortDirection.ASC)
            return list.OrderBy(property);
        else
            return list.OrderByDescending(property);
    }

    public static IOrderedEnumerable<T> ThenSort<T, TKey>(this IOrderedEnumerable<T> list, Func<T, TKey> property, SortDirection direction = SortDirection.ASC)
    {
        if (direction == SortDirection.ASC)
            return list.ThenBy(property);
        else
            return list.ThenByDescending(property);
    }
}

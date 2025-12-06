using NuvTools.Data.Sorting.Enumerations;
using System.Linq.Expressions;

namespace NuvTools.Data.Sorting;

/// <summary>
/// Extension methods for sorting collections in both ascending and descending order.
/// </summary>
public static class SortingExtensions
{

    /// <summary>
    /// Sorts a queryable collection by the specified property and direction.
    /// </summary>
    /// <typeparam name="T">The type of items in the queryable collection.</typeparam>
    /// <typeparam name="TKey">The type of the property to sort by.</typeparam>
    /// <param name="list">The source queryable collection.</param>
    /// <param name="property">An expression that identifies the property to sort by.</param>
    /// <param name="direction">The sort direction (ascending or descending). Defaults to ascending.</param>
    /// <returns>An ordered queryable collection.</returns>
    public static IOrderedQueryable<T> Sort<T, TKey>(this IQueryable<T> list, Expression<Func<T, TKey>> property, SortDirection direction = SortDirection.ASC)
    {
        if (direction == SortDirection.ASC)
            return list.OrderBy(property);
        else
            return list.OrderByDescending(property);
    }

    /// <summary>
    /// Performs a secondary sort on an already ordered queryable collection.
    /// </summary>
    /// <typeparam name="T">The type of items in the queryable collection.</typeparam>
    /// <typeparam name="TKey">The type of the property to sort by.</typeparam>
    /// <param name="list">The source ordered queryable collection.</param>
    /// <param name="property">An expression that identifies the property to sort by.</param>
    /// <param name="direction">The sort direction (ascending or descending). Defaults to ascending.</param>
    /// <returns>An ordered queryable collection with the secondary sort applied.</returns>
    public static IOrderedQueryable<T> ThenSort<T, TKey>(this IOrderedQueryable<T> list, Expression<Func<T, TKey>> property, SortDirection direction = SortDirection.ASC)
    {
        if (direction == SortDirection.ASC)
            return list.ThenBy(property);
        else
            return list.ThenByDescending(property);
    }

    /// <summary>
    /// Sorts an enumerable collection by the specified property and direction.
    /// </summary>
    /// <typeparam name="T">The type of items in the enumerable collection.</typeparam>
    /// <typeparam name="TKey">The type of the property to sort by.</typeparam>
    /// <param name="list">The source enumerable collection.</param>
    /// <param name="property">A function that identifies the property to sort by.</param>
    /// <param name="direction">The sort direction (ascending or descending). Defaults to ascending.</param>
    /// <returns>An ordered enumerable collection.</returns>
    public static IOrderedEnumerable<T> Sort<T, TKey>(this IEnumerable<T> list, Func<T, TKey> property, SortDirection direction = SortDirection.ASC)
    {
        if (direction == SortDirection.ASC)
            return list.OrderBy(property);
        else
            return list.OrderByDescending(property);
    }

    /// <summary>
    /// Performs a secondary sort on an already ordered enumerable collection.
    /// </summary>
    /// <typeparam name="T">The type of items in the enumerable collection.</typeparam>
    /// <typeparam name="TKey">The type of the property to sort by.</typeparam>
    /// <param name="list">The source ordered enumerable collection.</param>
    /// <param name="property">A function that identifies the property to sort by.</param>
    /// <param name="direction">The sort direction (ascending or descending). Defaults to ascending.</param>
    /// <returns>An ordered enumerable collection with the secondary sort applied.</returns>
    public static IOrderedEnumerable<T> ThenSort<T, TKey>(this IOrderedEnumerable<T> list, Func<T, TKey> property, SortDirection direction = SortDirection.ASC)
    {
        if (direction == SortDirection.ASC)
            return list.ThenBy(property);
        else
            return list.ThenByDescending(property);
    }
}

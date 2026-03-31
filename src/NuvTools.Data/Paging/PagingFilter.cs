using NuvTools.Data.Paging.Enumerations;
using NuvTools.Data.Sorting.Enumerations;

namespace NuvTools.Data.Paging;

/// <summary>
/// Represents a filter for paging operations containing page index and page size.
/// </summary>
public class PagingFilter
{
    /// <summary>
    /// Gets or sets the page index (0-indexed). Defaults to 0.
    /// </summary>
    public int PageIndex { get; set; } = 0;

    /// <summary>
    /// Gets or sets the number of items per page. Defaults to 30.
    /// </summary>
    public int PageSize { get; set; } = 30;

    /// <summary>
    /// Gets or sets the count mode for paging operations. Defaults to <see cref="PagingCountMode.Normal"/>.
    /// Use <see cref="PagingCountMode.SkipCount"/> for large datasets where COUNT(*) is too slow.
    /// </summary>
    public PagingCountMode CountMode { get; set; } = PagingCountMode.Normal;
}

/// <summary>
/// Represents a filter for paging operations with sorting capabilities.
/// </summary>
/// <typeparam name="T">Enum type representing the available columns to sort by.</typeparam>
public class PagingFilter<T> : PagingFilter where T : Enum
{
    /// <summary>
    /// Gets or sets the column to sort by.
    /// </summary>
    public required T SortColumn { get; set; }

    /// <summary>
    /// Gets or sets the sort direction (ascending or descending). Defaults to ascending.
    /// </summary>
    public SortDirection SortDirection { get; set; } = SortDirection.ASC;
}

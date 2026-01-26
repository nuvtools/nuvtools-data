using NuvTools.Data.Sorting.Enumerations;

namespace NuvTools.Data.Paging;

/// <summary>
/// Represents a filter for paging operations containing page number and page size.
/// </summary>
public class PagingFilter
{
    /// <summary>
    /// Gets or sets the page number (1-indexed). Defaults to 1.
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Gets or sets the number of items per page. Defaults to 30.
    /// </summary>
    public int PageSize { get; set; } = 30;

    /// <summary>
    /// Gets or sets the paging options for controlling count behavior.
    /// When null, the default behavior (always count) is used.
    /// </summary>
    public PagingOptions? Options { get; set; }
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

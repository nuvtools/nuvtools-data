using static NuvTools.Data.Sorting.SortingExtensions;

namespace NuvTools.Data.Paging;

/// <summary>
/// Filter for paging purpose.
/// </summary>
public class PagingFilter
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 30;
}

/// <summary>
/// Filter for paging purpose.
/// </summary>
/// <typeparam name="T">Enum type with column option to sort.</typeparam>
public class PagingFilter<T> : PagingFilter where T : Enum
{
    public required T SortColumn { get; set; }
    public SortDirection SortDirection { get; set; } = SortDirection.ASC;
}

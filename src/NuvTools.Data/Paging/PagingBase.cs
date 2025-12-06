namespace NuvTools.Data.Paging;

/// <summary>
/// Base class for paging results with collection data.
/// </summary>
/// <typeparam name="T">The type of collection (IQueryable or IEnumerable).</typeparam>
/// <typeparam name="R">The type of items in the collection.</typeparam>
public abstract class PagingBase<T, R> where T : IEnumerable<R>
{
    /// <summary>
    /// Gets or sets the current page number (1-indexed).
    /// </summary>
    public required int PageNumber { get; set; }

    /// <summary>
    /// Gets or sets the total number of items across all pages.
    /// </summary>
    public required int Total { get; set; }

    /// <summary>
    /// Gets or sets the collection of items for the current page.
    /// </summary>
    public required T List { get; set; }
}

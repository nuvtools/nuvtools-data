namespace NuvTools.Data.Paging;

/// <summary>
/// Base class for paging results with optional total count and HasMore indicator.
/// Use this class when you need to optimize large dataset paging by skipping count queries.
/// </summary>
/// <typeparam name="T">The type of collection (IQueryable or IEnumerable).</typeparam>
/// <typeparam name="R">The type of items in the collection.</typeparam>
public abstract class PagingResult<T, R> where T : IEnumerable<R>
{
    /// <summary>
    /// Gets or sets the current page number (1-indexed).
    /// </summary>
    public required int PageNumber { get; set; }

    /// <summary>
    /// Gets or sets the total number of items across all pages.
    /// Null when the count was skipped or unknown.
    /// </summary>
    public int? Total { get; set; }

    /// <summary>
    /// Gets or sets whether there are more pages available.
    /// This is populated when using the HasMore count mode (fetch N+1 pattern).
    /// Null when not using the HasMore mode.
    /// </summary>
    public bool? HasMore { get; set; }

    /// <summary>
    /// Gets or sets the collection of items for the current page.
    /// </summary>
    public required T List { get; set; }

    /// <summary>
    /// Gets a value indicating whether the total count is known.
    /// </summary>
    public bool IsTotalKnown => Total.HasValue;

    /// <summary>
    /// Gets a value indicating whether there might be more pages.
    /// Returns true if HasMore is true or if Total indicates more pages exist.
    /// Returns null if neither Total nor HasMore is available.
    /// </summary>
    /// <param name="pageSize">The page size to use for calculation.</param>
    /// <returns>True if more pages exist, false if not, null if unknown.</returns>
    public bool? HasMorePages(int pageSize)
    {
        if (HasMore.HasValue)
            return HasMore.Value;

        if (Total.HasValue)
            return PageNumber * pageSize < Total.Value;

        return null;
    }
}

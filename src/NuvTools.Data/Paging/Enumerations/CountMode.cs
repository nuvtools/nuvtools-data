namespace NuvTools.Data.Paging.Enumerations;

/// <summary>
/// Specifies how the total count should be calculated during paging operations.
/// </summary>
public enum CountMode
{
    /// <summary>
    /// Always execute a COUNT query to get the exact total (default behavior).
    /// This is the most expensive option for large datasets.
    /// </summary>
    Always = 0,

    /// <summary>
    /// Skip the count query entirely. Total will be null.
    /// Use this for infinite scroll or "load more" patterns where the total is not needed.
    /// </summary>
    Skip = 1,

    /// <summary>
    /// Fetch N+1 records to determine if more pages exist without running a count query.
    /// HasMore will be set based on whether the extra record exists.
    /// </summary>
    HasMore = 2,

    /// <summary>
    /// Count only up to a specified threshold (e.g., 10,000).
    /// Use this to show "10,000+" results instead of the exact count for large datasets.
    /// </summary>
    Threshold = 3,

    /// <summary>
    /// Use database system metadata to get an approximate row count instantly.
    /// Requires database-specific paging extensions (SQL Server or PostgreSQL).
    /// </summary>
    Approximate = 4
}

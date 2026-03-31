namespace NuvTools.Data.Paging.Enumerations;

/// <summary>
/// Specifies how the total count is determined during paging operations.
/// </summary>
public enum PagingCountMode
{
    /// <summary>
    /// Performs a full COUNT(*) query to determine the total number of items.
    /// This is the default behavior and provides an exact total.
    /// </summary>
    Normal = 0,

    /// <summary>
    /// Skips the COUNT(*) query for better performance on large datasets.
    /// Instead, fetches one extra item to determine if a next page exists.
    /// The <see cref="PagingBase{T, R}.Total"/> property will be set to -1 to indicate the count is unknown.
    /// </summary>
    SkipCount = 1
}

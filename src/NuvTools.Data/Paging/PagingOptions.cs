using NuvTools.Data.Paging.Enumerations;

namespace NuvTools.Data.Paging;

/// <summary>
/// Configuration options for paging operations, allowing optimization of count queries.
/// </summary>
public class PagingOptions
{
    /// <summary>
    /// Gets or sets the count mode to use when calculating the total.
    /// Defaults to <see cref="CountMode.Always"/>.
    /// </summary>
    public CountMode CountMode { get; set; } = CountMode.Always;

    /// <summary>
    /// Gets or sets the maximum count threshold when using <see cref="CountMode.Threshold"/>.
    /// If the count exceeds this value, the total will be capped at this threshold.
    /// Defaults to 10,000.
    /// </summary>
    public int CountThreshold { get; set; } = 10000;

    /// <summary>
    /// Gets or sets a pre-calculated total to use instead of executing a count query.
    /// When set to a non-null value, the count query is skipped entirely.
    /// This is useful when the total is cached or already known from a previous operation.
    /// </summary>
    public int? PreCalculatedTotal { get; set; }

    /// <summary>
    /// A delegate that provides the approximate row count for a table.
    /// Used when CountMode is set to Approximate.
    /// </summary>
    public Func<CancellationToken, Task<long>>? ApproximateCountProvider { get; set; }

    /// <summary>
    /// Creates a new instance with default settings (always count).
    /// </summary>
    public static PagingOptions Default => new();

    /// <summary>
    /// Creates options configured to skip the count query.
    /// </summary>
    public static PagingOptions SkipCount => new() { CountMode = CountMode.Skip };

    /// <summary>
    /// Creates options configured to use the HasMore pattern (fetch N+1).
    /// </summary>
    public static PagingOptions UseHasMore => new() { CountMode = CountMode.HasMore };

    /// <summary>
    /// Creates options configured to use approximate count from database system metadata.
    /// Requires database-specific paging extensions (SQL Server or PostgreSQL).
    /// </summary>
    public static PagingOptions UseApproximate => new() { CountMode = CountMode.Approximate };

    /// <summary>
    /// Creates options with a pre-calculated total.
    /// </summary>
    /// <param name="total">The known total count.</param>
    /// <returns>A new <see cref="PagingOptions"/> instance with the pre-calculated total.</returns>
    public static PagingOptions WithTotal(int total) => new() { PreCalculatedTotal = total };

    /// <summary>
    /// Creates options configured to count up to a specified threshold.
    /// </summary>
    /// <param name="threshold">The maximum count threshold.</param>
    /// <returns>A new <see cref="PagingOptions"/> instance with threshold counting.</returns>
    public static PagingOptions WithThreshold(int threshold) => new()
    {
        CountMode = CountMode.Threshold,
        CountThreshold = threshold
    };

    /// <summary>
    /// Creates options configured to use approximate count with a provider delegate.
    /// </summary>
    /// <param name="provider">A delegate that returns the approximate row count.</param>
    /// <returns>A new <see cref="PagingOptions"/> instance configured for approximate counting.</returns>
    public static PagingOptions WithApproximateCount(Func<CancellationToken, Task<long>> provider) => new()
    {
        CountMode = CountMode.Approximate,
        ApproximateCountProvider = provider
    };
}

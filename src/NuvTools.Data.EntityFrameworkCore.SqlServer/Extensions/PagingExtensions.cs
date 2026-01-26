using Microsoft.EntityFrameworkCore;
using NuvTools.Data.Paging;
using NuvTools.Data.Paging.Enumerations;

namespace NuvTools.Data.EntityFrameworkCore.SqlServer.Extensions;

/// <summary>
/// Extension methods for paging with SQL Server-specific approximate count support.
/// </summary>
public static class PagingExtensions
{
    /// <summary>
    /// Creates PagingOptions configured to use SQL Server approximate count.
    /// The approximate count is retrieved from SQL Server system metadata (sys.partitions) which is nearly instant.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to count.</typeparam>
    /// <param name="context">The DbContext instance used for approximate count queries.</param>
    /// <returns>A PagingOptions instance configured for approximate counting with SQL Server.</returns>
    /// <exception cref="ArgumentNullException">Thrown when context is null.</exception>
    public static PagingOptions CreateApproximateCountOptions<TEntity>(this DbContext context) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(context);

        return PagingOptions.WithApproximateCount(ct => context.GetApproximateCountAsync<TEntity>(ct));
    }

    /// <summary>
    /// Creates PagingOptions configured to use SQL Server approximate count for a specific table.
    /// The approximate count is retrieved from SQL Server system metadata (sys.partitions) which is nearly instant.
    /// </summary>
    /// <param name="context">The DbContext instance used for approximate count queries.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="schema">The schema name (defaults to "dbo").</param>
    /// <returns>A PagingOptions instance configured for approximate counting with SQL Server.</returns>
    /// <exception cref="ArgumentNullException">Thrown when context or tableName is null.</exception>
    public static PagingOptions CreateApproximateCountOptions(this DbContext context, string tableName, string schema = "dbo")
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(tableName);

        return PagingOptions.WithApproximateCount(ct => context.GetApproximateCountAsync(tableName, schema, ct));
    }
}

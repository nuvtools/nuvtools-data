using Microsoft.EntityFrameworkCore;
using NuvTools.Data.Paging;
using NuvTools.Data.Paging.Enumerations;

namespace NuvTools.Data.EntityFrameworkCore.PostgreSQL.Extensions;

/// <summary>
/// Extension methods for paging with PostgreSQL-specific approximate count support.
/// </summary>
public static class PagingExtensions
{
    /// <summary>
    /// Creates PagingOptions configured to use PostgreSQL approximate count.
    /// The approximate count is retrieved from PostgreSQL system metadata (pg_class.reltuples) which is nearly instant.
    /// Note: Returns estimate from last ANALYZE. Run ANALYZE for fresh stats.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to count.</typeparam>
    /// <param name="context">The DbContext instance used for approximate count queries.</param>
    /// <returns>A PagingOptions instance configured for approximate counting with PostgreSQL.</returns>
    /// <exception cref="ArgumentNullException">Thrown when context is null.</exception>
    public static PagingOptions CreateApproximateCountOptions<TEntity>(this DbContext context) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(context);

        return PagingOptions.WithApproximateCount(ct => context.GetApproximateCountAsync<TEntity>(ct));
    }

    /// <summary>
    /// Creates PagingOptions configured to use PostgreSQL approximate count for a specific table.
    /// The approximate count is retrieved from PostgreSQL system metadata (pg_class.reltuples) which is nearly instant.
    /// Note: Returns estimate from last ANALYZE. Run ANALYZE for fresh stats.
    /// </summary>
    /// <param name="context">The DbContext instance used for approximate count queries.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="schema">The schema name (defaults to "public").</param>
    /// <returns>A PagingOptions instance configured for approximate counting with PostgreSQL.</returns>
    /// <exception cref="ArgumentNullException">Thrown when context or tableName is null.</exception>
    public static PagingOptions CreateApproximateCountOptions(this DbContext context, string tableName, string schema = "public")
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(tableName);

        return PagingOptions.WithApproximateCount(ct => context.GetApproximateCountAsync(tableName, schema, ct));
    }
}

using Microsoft.EntityFrameworkCore;

namespace NuvTools.Data.EntityFrameworkCore.PostgreSQL.Extensions;

/// <summary>
/// Extension methods for DbContext specific to PostgreSQL.
/// </summary>
public static class DbContextExtensions
{
    /// <summary>
    /// Gets an approximate row count for an entity from PostgreSQL system metadata.
    /// This is nearly instant compared to COUNT(*) on large tables.
    /// Note: Returns estimate from last ANALYZE. Run ANALYZE for fresh stats.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to count.</typeparam>
    /// <param name="context">The DbContext instance.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The approximate row count.</returns>
    public static async Task<long> GetApproximateCountAsync<TEntity>(
        this DbContext context,
        CancellationToken cancellationToken = default) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(context);

        var entityType = context.Model.FindEntityType(typeof(TEntity));
        var tableName = entityType?.GetTableName();
        var schema = entityType?.GetSchema() ?? "public";

        if (string.IsNullOrEmpty(tableName))
            return 0;

        return await context.Database
            .SqlQuery<long>($@"
                SELECT COALESCE(c.reltuples, 0)::bigint
                FROM pg_class c
                INNER JOIN pg_namespace n ON c.relnamespace = n.oid
                WHERE c.relname = {tableName}
                  AND n.nspname = {schema}")
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Gets an approximate row count by table name from PostgreSQL system metadata.
    /// This is nearly instant compared to COUNT(*) on large tables.
    /// Note: Returns estimate from last ANALYZE. Run ANALYZE for fresh stats.
    /// </summary>
    /// <param name="context">The DbContext instance.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="schema">The schema name (defaults to "public").</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The approximate row count.</returns>
    public static async Task<long> GetApproximateCountAsync(
        this DbContext context,
        string tableName,
        string schema = "public",
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(tableName);

        return await context.Database
            .SqlQuery<long>($@"
                SELECT COALESCE(c.reltuples, 0)::bigint
                FROM pg_class c
                INNER JOIN pg_namespace n ON c.relnamespace = n.oid
                WHERE c.relname = {tableName}
                  AND n.nspname = {schema}")
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}

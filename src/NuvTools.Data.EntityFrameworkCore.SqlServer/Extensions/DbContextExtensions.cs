using Microsoft.EntityFrameworkCore;

namespace NuvTools.Data.EntityFrameworkCore.SqlServer.Extensions;

/// <summary>
/// Extension methods for DbContext specific to SQL Server.
/// </summary>
public static class DbContextExtensions
{
    /// <summary>
    /// Gets an approximate row count for an entity from SQL Server system metadata.
    /// This is nearly instant compared to COUNT(*) on large tables.
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
        var schema = entityType?.GetSchema() ?? "dbo";

        if (string.IsNullOrEmpty(tableName))
            return 0;

        return await context.Database
            .SqlQuery<long>($@"
                SELECT ISNULL(SUM(p.rows), 0)
                FROM sys.partitions p
                INNER JOIN sys.tables t ON p.object_id = t.object_id
                INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
                WHERE t.name = {tableName}
                  AND s.name = {schema}
                  AND p.index_id IN (0, 1)")
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Gets an approximate row count by table name from SQL Server system metadata.
    /// This is nearly instant compared to COUNT(*) on large tables.
    /// </summary>
    /// <param name="context">The DbContext instance.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="schema">The schema name (defaults to "dbo").</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The approximate row count.</returns>
    public static async Task<long> GetApproximateCountAsync(
        this DbContext context,
        string tableName,
        string schema = "dbo",
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(tableName);

        return await context.Database
            .SqlQuery<long>($@"
                SELECT ISNULL(SUM(p.rows), 0)
                FROM sys.partitions p
                INNER JOIN sys.tables t ON p.object_id = t.object_id
                INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
                WHERE t.name = {tableName}
                  AND s.name = {schema}
                  AND p.index_id IN (0, 1)")
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}

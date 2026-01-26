using Microsoft.EntityFrameworkCore;
using Npgsql;
using NuvTools.Data.Paging;
using NuvTools.Data.Paging.Enumerations;

namespace NuvTools.Data.EntityFrameworkCore.PostgreSQL.Paging;

/// <summary>
/// PostgreSQL-specific paging extensions that auto-wire approximate count using pg_class.reltuples.
/// </summary>
public static class PagingExtensions
{
    private const string ApproximateCountSql = @"
        SELECT COALESCE(c.reltuples, 0)::bigint
        FROM pg_class c
        JOIN pg_namespace n ON n.oid = c.relnamespace
        WHERE c.relname = @tableName AND n.nspname = @schema AND c.relkind = 'r'";
    /// <summary>
    /// Wraps a queryable into a paged result with automatic approximate count support for PostgreSQL.
    /// When <see cref="CountMode.Approximate"/> is used and no provider is configured, automatically
    /// queries pg_class.reltuples for fast approximate row counts.
    /// </summary>
    /// <typeparam name="TEntity">The entity type being queried.</typeparam>
    /// <param name="list">The source queryable collection.</param>
    /// <param name="context">The DbContext instance used to get table metadata and execute approximate count queries.</param>
    /// <param name="pageIndex">The page index (0-indexed).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="options">The paging options controlling count behavior.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation, containing a paged result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when list, context, or options is null.</exception>
    /// <example>
    /// <code>
    /// // Simple usage - auto-wires approximate count
    /// var result = await query.PagingWrapAsync(context, 0, 30, PagingOptions.UseApproximate);
    /// </code>
    /// </example>
    public static async Task<PagingQueryableResult<TEntity>> PagingWrapAsync<TEntity>(
        this IQueryable<TEntity> list,
        DbContext context,
        int pageIndex,
        int pageSize,
        PagingOptions options,
        CancellationToken cancellationToken = default) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(list);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(options);

        // Auto-wire approximate count if needed
        if (options.CountMode == CountMode.Approximate && options.ApproximateCountProvider == null)
        {
            options = PagingOptions.WithApproximateCount(
                ct => GetApproximateCountAsync<TEntity>(context, ct));
        }

        return await EntityFrameworkCore.Paging.PagingExtensions
            .PagingWrapAsync(list, pageIndex, pageSize, options, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the approximate row count for an entity type using PostgreSQL's pg_class.reltuples.
    /// This is faster than COUNT(*) for large tables as it reads from statistics metadata.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to get the count for.</typeparam>
    /// <param name="context">The DbContext instance.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The approximate row count from database metadata.</returns>
    public static async Task<long> GetApproximateCountAsync<TEntity>(
        DbContext context,
        CancellationToken cancellationToken = default) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(context);

        var entityType = context.Model.FindEntityType(typeof(TEntity));
        var tableName = entityType?.GetTableName() ?? typeof(TEntity).Name;
        var schema = entityType?.GetSchema();

        return await GetApproximateCountAsync(context, tableName, schema, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the approximate row count for a table using PostgreSQL's pg_class.reltuples.
    /// This is faster than COUNT(*) for large tables as it reads from statistics metadata.
    /// </summary>
    /// <param name="context">The DbContext instance.</param>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="schema">The schema name (optional, defaults to "public" if not specified).</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The approximate row count from database metadata.</returns>
    public static async Task<long> GetApproximateCountAsync(
        DbContext context,
        string tableName,
        string? schema = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(tableName);

        var tableNameParam = new NpgsqlParameter("@tableName", tableName);
        var schemaParam = new NpgsqlParameter("@schema", schema ?? "public");

        var result = await context.Database
            .SqlQueryRaw<long>(ApproximateCountSql, tableNameParam, schemaParam)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return result;
    }
}

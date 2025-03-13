using Microsoft.EntityFrameworkCore;
using NuvTools.Common.ResultWrapper;
using NuvTools.Common.Strings;

namespace NuvTools.Data.EntityFrameworkCore.Extensions;

public static class DbContextExtensions
{
    private const string ENTITY_WITH_KEYS_NOT_FOUND = "Entity with keys {0} not found.";
    private const string AT_LEAST_ONE_KEY_MUST_BE_PROVIDED = "At least one key value must be provided.";

    public static async Task<IResult<TKey>> AddAndSaveAsync<TEntity, TKey>(this DbContext context, TEntity entity, CancellationToken cancellationToken = default) where TEntity : class
    {
        var result = await context.AddAndSaveWithCompositeKeyAsync(entity, cancellationToken);
        return Result<TKey>.Success((TKey)result.Data![0]);
    }

    public static async Task<IResult<object[]>> AddAndSaveWithCompositeKeyAsync<TEntity>(this DbContext context, TEntity entity, CancellationToken cancellationToken = default) where TEntity : class
    {
        try
        {
            context.Set<TEntity>().Add(entity);
            await context.SaveChangesAsync(cancellationToken);

            return Result<object[]>.Success(context.FindPrimaryKeyValues(entity));
        }
        catch (DbUpdateException ex)
        {
            return Result<object[]>.Fail(ex);
        }
    }

    public static object[] FindPrimaryKeyValues<TEntity>(this DbContext context, TEntity entity) where TEntity : class
    {
        var entry = context.Entry(entity);

        object[] keyValues = [.. entry.Metadata.FindPrimaryKey()!
                     .Properties
                     .Select(p => entry.Property(p.Name).CurrentValue!)];

        return keyValues;
    }

    public static async Task<IResult> UpdateAndSaveAsync<TEntity>(this DbContext context, TEntity entity, params object[] keyValues) where TEntity : class
    {
        return await context.UpdateAndSaveAsync(entity, keyValues, CancellationToken.None);
    }

    public static async Task<IResult> UpdateAndSaveAsync<TEntity>(this DbContext context, TEntity entity, object[] keyValues, CancellationToken cancellationToken = default) where TEntity : class
    {
        if (keyValues is null || keyValues.Length == 0)
            return Result.ValidationFail(AT_LEAST_ONE_KEY_MUST_BE_PROVIDED);

        try
        {
            var existingItem = await context.Set<TEntity>().FindAsync(keyValues, cancellationToken)
                                ?? throw new InvalidOperationException(ENTITY_WITH_KEYS_NOT_FOUND.Format(string.Join(", ", keyValues)));
            context.Entry(existingItem).CurrentValues.SetValues(entity);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(ex);
        }
    }

    public static async Task<IResult> RemoveAndSaveAsync<TEntity>(this DbContext context, params object[] keyValues) where TEntity : class
    {
        return await context.RemoveAndSaveAsync<TEntity>(keyValues, CancellationToken.None);
    }

    public static async Task<IResult> RemoveAndSaveAsync<TEntity>(this DbContext context, object[] keyValues, CancellationToken cancellationToken = default) where TEntity : class
    {
        if (keyValues is null || keyValues.Length == 0)
            return Result.ValidationFail(AT_LEAST_ONE_KEY_MUST_BE_PROVIDED);

        try
        {
            var existingItem = await context.Set<TEntity>().FindAsync(keyValues, cancellationToken)
                                ?? throw new InvalidOperationException(ENTITY_WITH_KEYS_NOT_FOUND.Format(string.Join(", ", keyValues)));
            context.Set<TEntity>().Remove(existingItem);

            await context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(ex);
        }
    }


    /// <summary>
    /// Executes the provided action within the execution strategy of the DbContext.
    /// <para>
    /// <see href="https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency" />
    /// </para>
    /// </summary>
    /// <param name="context">The DbContext instance.</param>
    /// <param name="action">The action to execute.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public static async Task ExecuteWithStrategyAsync(this DbContext context,
                                                       Func<CancellationToken, Task> action,
                                                       CancellationToken cancellationToken = default)
    {
        var strategy = context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async ct =>
        {
            await action(ct);
        }, cancellationToken);
    }

    /// <summary>
    /// Executes the provided action within the execution strategy of the DbContext and returns a result.
    /// <para>
    /// <see href="https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency" />
    /// </para>
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="context">The DbContext instance.</param>
    /// <param name="action">The action to execute.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <returns>A Task representing the asynchronous operation, with a result of type T.</returns>
    public static async Task<T> ExecuteWithStrategyAsync<T>(this DbContext context,
                                                            Func<CancellationToken, Task<T>> action,
                                                            CancellationToken cancellationToken = default)
    {
        T result = default!;
        await context.ExecuteWithStrategyAsync(async ct =>
        {
            result = await action(ct);
        }, cancellationToken);
        return result;
    }

}
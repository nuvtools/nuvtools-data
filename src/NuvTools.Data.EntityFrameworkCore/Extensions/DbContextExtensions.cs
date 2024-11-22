using Microsoft.EntityFrameworkCore;
using NuvTools.Common.ResultWrapper;
using NuvTools.Common.Strings;

namespace NuvTools.Data.EntityFrameworkCore.Extensions;

public static class DbContextExtensions
{
    private const string ENTITY_WITH_KEYS_NOT_FOUND = "Entity with keys {0} not found.";
    private const string AT_LEAST_ONE_KEY_MUST_BE_PROVIDED = "At least one key value must be provided.";

    public static async Task<IResult<TKey>> AddAndSaveAsync<TEntity, TKey>(this DbContext context, TEntity entity) where TEntity : class
    {
        var result = await context.AddAndSaveWithCompositeKeyAsync(entity);
        return Result<TKey>.Success((TKey)result.Data![0]);
    }

    public static async Task<IResult<object[]>> AddAndSaveWithCompositeKeyAsync<TEntity>(this DbContext context, TEntity entity) where TEntity : class
    {
        try
        {
            context.Set<TEntity>().Add(entity);
            await context.SaveChangesAsync();

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

        object[] keyValues = entry.Metadata.FindPrimaryKey()!
                     .Properties
                     .Select(p => entry.Property(p.Name).CurrentValue!)
                     .ToArray();

        return keyValues;
    }

    public static async Task<IResult> UpdateAndSaveAsync<TEntity>(this DbContext context, TEntity entity, params object[] keyValues) where TEntity : class
    {
        if (keyValues is null || keyValues.Length == 0)
            return Result.ValidationFail(AT_LEAST_ONE_KEY_MUST_BE_PROVIDED);

        try
        {
            var existingItem = await context.Set<TEntity>().FindAsync(keyValues) 
                                ?? throw new InvalidOperationException(ENTITY_WITH_KEYS_NOT_FOUND.Format(string.Join(", ", keyValues)));
            context.Entry(existingItem).CurrentValues.SetValues(entity);
            await context.SaveChangesAsync();

            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(ex);
        }
    }

    public static async Task<IResult> RemoveAndSaveAsync<TEntity>(this DbContext context, params object[] keyValues) where TEntity : class
    {
        if (keyValues is null || keyValues.Length == 0)
            return Result.ValidationFail(AT_LEAST_ONE_KEY_MUST_BE_PROVIDED);

        try
        {
            var existingItem = await context.Set<TEntity>().FindAsync(keyValues) 
                                ?? throw new InvalidOperationException(ENTITY_WITH_KEYS_NOT_FOUND.Format(string.Join(", ", keyValues)));
            context.Set<TEntity>().Remove(existingItem);

            await context.SaveChangesAsync();

            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(ex);
        }
    }

    /// <summary>
    /// Executes action inside of Execution Strategy and Transaction. Note: Required when use EnableRetryOnFailure option.
    /// <para>
    /// <see href="https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency" />
    /// </para>
    /// </summary>
    /// <param name="context"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static async Task ExecuteResilientTransactionAsync(this DbContext context, Func<Task> action)
    {
        await context.ExecuteStrategyAsync(async () =>
        {
            using (var transaction = context.Database.BeginTransaction())
            {
                await action();
                transaction.Commit();
            }
        });
    }

    /// <summary>
    /// Executes action inside of Execution Strategy.
    /// <para>
    /// <see href="https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency" />
    /// </para>
    /// </summary>
    /// <param name="context"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static async Task ExecuteStrategyAsync(this DbContext context, Func<Task> action)
    {
        var strategy = context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(action);
    }
}
using NuvTools.Common.ResultWrapper;

namespace NuvTools.Data.EntityFrameworkCore.Context;

public interface IDbContextWithListCommands
{

    /// <summary>
    /// Synchronizes a DbSet with a provided list by adding, updating, or removing entities.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TKey">The type of the key used to uniquely identify entities.</typeparam>
    /// <param name="entities">The list of entities to synchronize with the database.</param>
    /// <param name="keySelector">A function to select the key for matching entities.</param>
    /// <param name="filter">An optional filter predicate to apply to the database entities.</param>
    /// <remarks>
    /// This method adds entities in the list that are not in the database, updates entities that exist in both,
    /// and removes entities in the database that are not in the list.
    /// </remarks>
    /// <example>
    /// await context.SyncFromListAsync(
    ///     updatedEmployees,
    ///     e => e.Id,
    ///     e => e.IsActive
    /// );
    /// </example>
    Task<IResult> SyncFromListAsync<TEntity, TKey>(
       IEnumerable<TEntity> entities,
       Func<TEntity, TKey> keySelector,
       Func<TEntity, bool>? filter = null)
       where TEntity : class
       where TKey : notnull;

    /// <summary>
    /// Synchronizes a DbSet with a provided list by adding or updating entities, without removing any entities.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TKey">The type of the key used to uniquely identify entities.</typeparam>
    /// <param name="entities">The list of entities to synchronize with the database.</param>
    /// <param name="keySelector">A function to select the key for matching entities.</param>
    /// <param name="filter">An optional filter predicate to apply to the database entities.</param>
    /// <remarks>
    /// This method adds entities in the list that are not in the database and updates entities that exist in both.
    /// It does not remove any entities from the database.
    /// </remarks>
    /// <example>
    /// await context.AddOrUpdateFromListAsync(
    ///     updatedProducts,
    ///     p => p.ProductCode,
    ///     p => p.IsActive
    /// );
    /// </example>
    Task<IResult> AddOrUpdateFromListAsync<TEntity, TKey>(
        IEnumerable<TEntity> entities,
        Func<TEntity, TKey> keySelector,
        Func<TEntity, bool>? filter = null)
        where TEntity : class
        where TKey : notnull;

    /// <summary>
    /// Synchronizes a DbSet with a provided list by adding or removing entities, without updating any entities.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TKey">The type of the key used to uniquely identify entities.</typeparam>
    /// <param name="entities">The list of entities to synchronize with the database.</param>
    /// <param name="keySelector">A function to select the key for matching entities.</param>
    /// <param name="filter">An optional filter predicate to apply to the database entities.</param>
    /// <remarks>
    /// This method adds entities in the list that are not in the database and removes entities in the database
    /// that are not in the list. It does not update any entities.
    /// </remarks>
    /// <example>
    /// await context.AddOrRemoveFromListAsync(
    ///     updatedOrders,
    ///     o => o.OrderId,
    ///     o => o.IsPending
    /// );
    /// </example>
    Task<IResult> AddOrRemoveFromListAsync<TEntity, TKey>(
        IEnumerable<TEntity> entities,
        Func<TEntity, TKey> keySelector,
        Func<TEntity, bool>? filter = null)
        where TEntity : class
        where TKey : notnull;
}
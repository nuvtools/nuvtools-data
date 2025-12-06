using Microsoft.EntityFrameworkCore.Storage;
using NuvTools.Common.ResultWrapper;

namespace NuvTools.Data.EntityFrameworkCore.Context;

/// <summary>
/// Defines common database context operations for transaction management, CRUD operations with Result pattern, and execution strategies.
/// </summary>
public interface IDbContextCommands
{
    /// <summary>
    /// Begins a new database transaction asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation, containing the database transaction.</returns>
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current transaction asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an action within the database execution strategy for connection resiliency.
    /// </summary>
    /// <param name="action">The action to execute within the strategy.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <seealso href="https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency"/>
    Task ExecuteWithStrategyAsync(Func<CancellationToken, Task> action,
                                        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an action within the database execution strategy for connection resiliency and returns a result.
    /// </summary>
    /// <typeparam name="T">The type of the result returned by the action.</typeparam>
    /// <param name="action">The action to execute within the strategy.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation, containing the result of the action.</returns>
    /// <seealso href="https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency"/>
    Task<T> ExecuteWithStrategyAsync<T>(Func<CancellationToken, Task<T>> action,
                                        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all changes made in this context to the database asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation, containing the number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current active transaction, or null if no transaction is active.
    /// </summary>
    IDbContextTransaction? CurrentTransaction { get; }

    /// <summary>
    /// Adds a new entity to the database and saves changes, returning the entity's primary key.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TKey">The type of the primary key.</typeparam>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation, containing the result with the primary key.</returns>
    Task<IResult<TKey>> AddAndSaveAsync<TEntity, TKey>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class;

    /// <summary>
    /// Updates an existing entity in the database and saves changes.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="entity">The entity with updated values.</param>
    /// <param name="keyValues">The primary key values of the entity to update.</param>
    /// <returns>A task that represents the asynchronous operation, containing the result of the update operation.</returns>
    Task<IResult> UpdateAndSaveAsync<TEntity>(TEntity entity, params object[] keyValues) where TEntity : class;

    /// <summary>
    /// Updates an existing entity in the database and saves changes.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="entity">The entity with updated values.</param>
    /// <param name="keyValues">The primary key values of the entity to update.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation, containing the result of the update operation.</returns>
    Task<IResult> UpdateAndSaveAsync<TEntity>(TEntity entity, object[] keyValues, CancellationToken cancellationToken = default) where TEntity : class;

    /// <summary>
    /// Adds a new entity with a composite key to the database and saves changes.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation, containing the result with the composite key values.</returns>
    Task<IResult<object[]>> AddAndSaveWithCompositeKeyAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class;

    /// <summary>
    /// Removes an entity from the database and saves changes.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="keyValues">The primary key values of the entity to remove.</param>
    /// <returns>A task that represents the asynchronous operation, containing the result of the remove operation.</returns>
    Task<IResult> RemoveAndSaveAsync<TEntity>(params object[] keyValues) where TEntity : class;

    /// <summary>
    /// Removes an entity from the database and saves changes.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="keyValues">The primary key values of the entity to remove.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation, containing the result of the remove operation.</returns>
    Task<IResult> RemoveAndSaveAsync<TEntity>(object[] keyValues, CancellationToken cancellationToken = default) where TEntity : class;
}
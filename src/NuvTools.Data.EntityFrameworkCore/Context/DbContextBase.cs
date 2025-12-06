using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using NuvTools.Common.ResultWrapper;
using System.Linq.Expressions;

namespace NuvTools.Data.EntityFrameworkCore.Context;

/// <summary>
/// Abstract base class for DbContext implementations that provides common CRUD operations,
/// transaction management, bulk operations, and execution strategies using the Result pattern.
/// </summary>
/// <remarks>
/// This class implements <see cref="IDbContextCommands"/> and <see cref="IDbContextWithListCommands"/>
/// to provide a consistent interface for database operations across the application.
/// </remarks>
public abstract class DbContextBase : DbContext, IDbContextCommands, IDbContextWithListCommands
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DbContextBase"/> class.
    /// </summary>
    protected DbContextBase()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DbContextBase"/> class with the specified options.
    /// </summary>
    /// <param name="options">The options to be used by the DbContext.</param>
    protected DbContextBase(DbContextOptions options) : base(options)
    {
    }

    /// <inheritdoc />
    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return Database.BeginTransactionAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task ExecuteWithStrategyAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
    {
        return Extensions.DbContextExtensions.ExecuteWithStrategyAsync(this, action, cancellationToken);
    }

    /// <inheritdoc />
    public Task<T> ExecuteWithStrategyAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default)
    {
        return Extensions.DbContextExtensions.ExecuteWithStrategyAsync(this, action, cancellationToken);
    }

    /// <inheritdoc />
    public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        return Database.CommitTransactionAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        return Database.RollbackTransactionAsync(cancellationToken);
    }

    /// <inheritdoc />
    public IDbContextTransaction? CurrentTransaction { get { return Database.CurrentTransaction; } }

    /// <inheritdoc />
    public Task<IResult<TKey>> AddAndSaveAsync<TEntity, TKey>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class
    {
        return Extensions.DbContextExtensions.AddAndSaveAsync<TEntity, TKey>(this, entity, cancellationToken);
    }

    /// <inheritdoc />
    public Task<IResult> UpdateAndSaveAsync<TEntity>(TEntity entity, params object[] keyValues) where TEntity : class
    {
        return Extensions.DbContextExtensions.UpdateAndSaveAsync(this, entity, keyValues);
    }

    /// <inheritdoc />
    public Task<IResult> UpdateAndSaveAsync<TEntity>(TEntity entity, object[] keyValues, CancellationToken cancellationToken = default) where TEntity : class
    {
        return Extensions.DbContextExtensions.UpdateAndSaveAsync(this, entity, keyValues, cancellationToken);
    }

    /// <inheritdoc />
    public Task<IResult> RemoveAndSaveAsync<TEntity>(params object[] keyValues) where TEntity : class
    {
        return Extensions.DbContextExtensions.RemoveAndSaveAsync<TEntity>(this, keyValues);
    }

    /// <inheritdoc />
    public Task<IResult> RemoveAndSaveAsync<TEntity>(object[] keyValues, CancellationToken cancellationToken = default) where TEntity : class
    {
        return Extensions.DbContextExtensions.RemoveAndSaveAsync<TEntity>(this, keyValues, cancellationToken);
    }

    /// <inheritdoc />
    public Task<IResult<object[]>> AddAndSaveWithCompositeKeyAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class
    {
        return Extensions.DbContextExtensions.AddAndSaveWithCompositeKeyAsync(this, entity, cancellationToken);
    }

    /// <inheritdoc />
    public Task<IResult> SyncFromListAsync<TEntity, TKey>(IEnumerable<TEntity> entities, Func<TEntity, TKey> keySelector,
                                                            Expression<Func<TEntity, bool>>? filter = null, CancellationToken cancellationToken = default)
        where TEntity : class
        where TKey : notnull
    {
        return Extensions.DbContextWithListExtensions.SyncFromListAsync(this, entities, keySelector, filter, cancellationToken);
    }

    /// <inheritdoc />
    public Task<IResult> AddOrUpdateFromListAsync<TEntity, TKey>(IEnumerable<TEntity> entities, Func<TEntity, TKey> keySelector,
                                                            Expression<Func<TEntity, bool>>? filter = null, CancellationToken cancellationToken = default)
        where TEntity : class
        where TKey : notnull
    {
        return Extensions.DbContextWithListExtensions.AddOrUpdateFromListAsync(this, entities, keySelector, filter, cancellationToken);
    }

    /// <inheritdoc />
    public Task<IResult> AddOrRemoveFromListAsync<TEntity, TKey>(IEnumerable<TEntity> entities, Func<TEntity, TKey> keySelector,
                                                            Expression<Func<TEntity, bool>>? filter = null, CancellationToken cancellationToken = default)
        where TEntity : class
        where TKey : notnull
    {
        return Extensions.DbContextWithListExtensions.AddOrRemoveFromListAsync(this, entities, keySelector, filter, cancellationToken);
    }

}

using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using NuvTools.Common.ResultWrapper;
using System.Linq.Expressions;

namespace NuvTools.Data.EntityFrameworkCore.Context;

public abstract class DbContextBase : DbContext, IDbContextCommands, IDbContextWithListCommands
{
    protected DbContextBase()
    {
    }

    protected DbContextBase(DbContextOptions options) : base(options)
    {
    }

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return Database.BeginTransactionAsync(cancellationToken);
    }
    public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        return Database.CommitTransactionAsync(cancellationToken);
    }
    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        return Database.RollbackTransactionAsync(cancellationToken);
    }

    public IDbContextTransaction? CurrentTransaction { get { return Database.CurrentTransaction; } }

    public Task<IResult<TKey>> AddAndSaveAsync<TEntity, TKey>(TEntity entity) where TEntity : class
    {
        return Extensions.DbContextExtensions.AddAndSaveAsync<TEntity, TKey>(this, entity);
    }

    public Task<IResult> UpdateAndSaveAsync<TEntity>(TEntity entity, params object[] keyValues) where TEntity : class
    {
        return Extensions.DbContextExtensions.UpdateAndSaveAsync(this, entity, keyValues);
    }

    public Task<IResult> RemoveAndSaveAsync<TEntity>(params object[] keyValues) where TEntity : class
    {
        return Extensions.DbContextExtensions.RemoveAndSaveAsync<TEntity>(this, keyValues);
    }

    public Task<IResult<object[]>> AddAndSaveWithCompositeKeyAsync<TEntity>(TEntity entity) where TEntity : class
    {
        return Extensions.DbContextExtensions.AddAndSaveWithCompositeKeyAsync(this, entity);
    }

    public Task<IResult> SyncFromListAsync<TEntity, TKey>(IEnumerable<TEntity> entities, Func<TEntity, TKey> keySelector, Expression<Func<TEntity, bool>>? filter = null)
        where TEntity : class
        where TKey : notnull
    {
        return Extensions.DbContextWithListExtensions.SyncFromListAsync(this, entities, keySelector, filter);
    }

    public Task<IResult> AddOrUpdateFromListAsync<TEntity, TKey>(IEnumerable<TEntity> entities, Func<TEntity, TKey> keySelector, Expression<Func<TEntity, bool>>? filter = null)
        where TEntity : class
        where TKey : notnull
    {
        return Extensions.DbContextWithListExtensions.AddOrUpdateFromListAsync(this, entities, keySelector, filter);
    }

    public Task<IResult> AddOrRemoveFromListAsync<TEntity, TKey>(IEnumerable<TEntity> entities, Func<TEntity, TKey> keySelector, Expression<Func<TEntity, bool>>? filter = null)
        where TEntity : class
        where TKey : notnull
    {
        return Extensions.DbContextWithListExtensions.AddOrRemoveFromListAsync(this, entities, keySelector, filter);
    }
}

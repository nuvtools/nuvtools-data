using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using NuvTools.Common.ResultWrapper;
using NuvTools.Data.EntityFrameworkCore.Context;

namespace NuvTools.Data.EntityFrameworkCore.SqlServer.Context;

public abstract class DbContextBase : DbContext, IDbContextCommands
{

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

    public IDbContextTransaction CurrentTransaction { get { return Database.CurrentTransaction; } }

    public Task<IResult<TKey>> AddAndSaveAsync<TEntity, TKey>(TEntity entity) where TEntity : class
    {
        return NuvTools.Data.EntityFrameworkCore.Extensions.DbContextExtensions.AddAndSaveAsync<TEntity, TKey>(this, entity);
    }

    public Task<IResult> UpdateAndSaveAsync<TEntity>(TEntity entity, params object[] keyValues) where TEntity : class
    {
        return NuvTools.Data.EntityFrameworkCore.Extensions.DbContextExtensions.UpdateAndSaveAsync(this, entity);
    }

    public Task<IResult> RemoveAndSaveAsync<TEntity>(params object[] keyValues) where TEntity : class
    {
        return NuvTools.Data.EntityFrameworkCore.Extensions.DbContextExtensions.RemoveAndSaveAsync<TEntity>(this, keyValues);
    }

    public Task<IResult<object[]>> AddAndSaveWithCompositeKeyAsync<TEntity>(TEntity entity) where TEntity : class
    {
        return NuvTools.Data.EntityFrameworkCore.Extensions.DbContextExtensions.AddAndSaveWithCompositeKeyAsync<TEntity>(this, entity);
    }
}

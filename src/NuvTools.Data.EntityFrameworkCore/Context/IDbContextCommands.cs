using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NuvTools.Common.ResultWrapper;

namespace NuvTools.Data.EntityFrameworkCore.Context;

public interface IDbContextCommands
{
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    Task<T> ExecuteWithStrategyAsync<T>(Func<CancellationToken, Task<T>> action,
                                        CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    IDbContextTransaction? CurrentTransaction { get; }

    Task<IResult<TKey>> AddAndSaveAsync<TEntity, TKey>(TEntity entity) where TEntity : class;

    Task<IResult> UpdateAndSaveAsync<TEntity>(TEntity entity, params object[] keyValues) where TEntity : class;

    Task<IResult<object[]>> AddAndSaveWithCompositeKeyAsync<TEntity>(TEntity entity) where TEntity : class;

    Task<IResult> RemoveAndSaveAsync<TEntity>(params object[] keyValues) where TEntity : class;
}
using Microsoft.EntityFrameworkCore;

namespace NuvTools.Data.EntityFrameworkCore.Context;

/// <summary>
/// Provider-specific implementation of the transaction-scoped lock behind
/// <see cref="IDbContextCommands.AcquireTransactionLockAsync"/>.
/// </summary>
/// <remarks>
/// Every relational database offers this, under a different name and keyed differently —
/// <c>pg_advisory_xact_lock</c> on PostgreSQL, <c>sp_getapplock</c> on SQL Server — so the statement lives in
/// the provider package, not here. The implementation is registered by that package's
/// <c>AddDatabase</c>/<c>AddDatabaseByConnectionName</c> helpers, and <see cref="DbContextBase"/> resolves it
/// from the application service provider.
/// </remarks>
public interface IDbContextTransactionLock
{
    /// <summary>
    /// Acquires the exclusive lock named <paramref name="name"/>, blocking until it is granted, and holds it
    /// until the current transaction of <paramref name="context"/> ends.
    /// </summary>
    /// <param name="context">The context whose transaction and connection the lock belongs to.</param>
    /// <param name="name">Lock name, as given by the caller.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that completes once the lock is held.</returns>
    Task AcquireAsync(DbContext context, string name, CancellationToken cancellationToken = default);
}

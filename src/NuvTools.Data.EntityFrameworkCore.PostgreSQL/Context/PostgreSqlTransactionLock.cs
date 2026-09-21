using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using NuvTools.Data.EntityFrameworkCore.Context;

namespace NuvTools.Data.EntityFrameworkCore.PostgreSQL.Context;

/// <summary>
/// PostgreSQL implementation of <see cref="IDbContextTransactionLock"/>, on top of
/// <c>pg_advisory_xact_lock</c>: blocks until the lock is granted and releases it when the transaction ends.
/// </summary>
/// <remarks>
/// PostgreSQL keys advisory locks by a 64-bit integer, so the caller's name is hashed into one. The hash is
/// SHA-256 rather than <see cref="string.GetHashCode()"/>, which is randomized per process — two replicas would
/// derive different keys for the same name and neither would block the other.
/// </remarks>
public sealed class PostgreSqlTransactionLock : IDbContextTransactionLock
{
    /// <inheritdoc />
    public Task AcquireAsync(DbContext context, string name, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return context.Database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_xact_lock({0})", [ToKey(name)], cancellationToken);
    }

    private static long ToKey(string name)
        => BitConverter.ToInt64(SHA256.HashData(Encoding.UTF8.GetBytes(name)), 0);
}

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using NuvTools.Data.EntityFrameworkCore.Context;

namespace NuvTools.Data.EntityFrameworkCore.SqlServer.Context;

/// <summary>
/// SQL Server implementation of <see cref="IDbContextTransactionLock"/>, on top of <c>sp_getapplock</c> with
/// <c>@LockOwner = 'Transaction'</c>: blocks until the lock is granted and releases it when the transaction ends.
/// </summary>
/// <remarks>
/// SQL Server keys application locks by the resource name itself, so the caller's name is used as given —
/// only names longer than the 255 characters it accepts are replaced by a hash.
/// </remarks>
public sealed class SqlServerTransactionLock : IDbContextTransactionLock
{
    /// <summary>Maximum length of <c>@Resource</c> in <c>sp_getapplock</c>.</summary>
    private const int ResourceMaxLength = 255;

    /// <summary>
    /// Waits indefinitely, matching <c>pg_advisory_xact_lock</c>, which has no timeout. How long the caller is
    /// willing to wait is governed by the command timeout and the cancellation token.
    /// </summary>
    private const int LockTimeoutMilliseconds = -1;

    /// <inheritdoc />
    public Task AcquireAsync(DbContext context, string name, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return context.Database.ExecuteSqlRawAsync(
            "EXEC sp_getapplock @Resource = {0}, @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = {1}",
            [ToResource(name), LockTimeoutMilliseconds],
            cancellationToken);
    }

    private static string ToResource(string name)
        => name.Length <= ResourceMaxLength
            ? name
            : BitConverter.ToInt64(SHA256.HashData(Encoding.UTF8.GetBytes(name)), 0).ToString(CultureInfo.InvariantCulture);
}

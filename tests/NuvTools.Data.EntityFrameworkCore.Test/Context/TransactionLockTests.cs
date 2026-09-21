using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using NuvTools.Data.EntityFrameworkCore.Context;

namespace NuvTools.Data.EntityFrameworkCore.Test.Context;

public class LockTestContext(DbContextOptions options) : DbContextBase(options)
{
}

[TestFixture]
public class TransactionLockTests
{
    private static LockTestContext InMemoryContext()
        => new(new DbContextOptionsBuilder<LockTestContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    /// <summary>
    /// Sem pacote de provider registrado — contexto em memória montado à mão, como nos testes — não há
    /// implementação de lock e nada a serializar, então a chamada é inócua. É o que permite testar quem usa
    /// o lock sem exigir um banco real.
    /// </summary>
    [Test]
    public async Task WithoutRegisteredProviderLockDoesNothing()
    {
        using var context = InMemoryContext();

        // Sem transação e sem implementação registrada: não deve lançar.
        await context.AcquireTransactionLockAsync("qualquer-nome");

        Assert.Pass();
    }

    [Test]
    public void EmptyNameIsRejected()
    {
        using var context = InMemoryContext();

        Assert.ThrowsAsync<ArgumentException>(async () => await context.AcquireTransactionLockAsync(" "));
    }

    [Test]
    public void NullNameIsRejected()
    {
        using var context = InMemoryContext();

        Assert.ThrowsAsync<ArgumentNullException>(async () => await context.AcquireTransactionLockAsync(null!));
    }
}

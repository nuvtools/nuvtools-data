using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using NuvTools.Data.EntityFrameworkCore.Paging;
using NuvTools.Data.Paging;
using NuvTools.Data.Paging.Enumerations;

namespace NuvTools.Data.EntityFrameworkCore.Test.Paging;

[TestFixture]
public class PagingExtensionsTests
{
    private TestDbContext _context = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TestDbContext(options);

        // Seed test data: 100 items
        for (var i = 1; i <= 100; i++)
        {
            _context.TestEntities.Add(new TestEntity { Id = i, Name = $"Entity {i}" });
        }
        _context.SaveChanges();
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    #region PagingWrapAsync Tests

    [Test]
    public async Task PagingWrapAsync_FirstPage_ReturnsFirstItems()
    {
        // Arrange
        var query = _context.TestEntities.OrderBy(e => e.Id);
        var pageIndex = 0;
        var pageSize = 10;

        // Act
        var result = await query.PagingWrapAsync(pageIndex, pageSize);

        // Assert
        Assert.That(result.PageIndex, Is.EqualTo(0));
        Assert.That(result.Total, Is.EqualTo(100));
        var list = await result.List.ToListAsync();
        Assert.That(list.Count, Is.EqualTo(10));
        Assert.That(list.First().Id, Is.EqualTo(1));
        Assert.That(list.Last().Id, Is.EqualTo(10));
    }

    [Test]
    public async Task PagingWrapAsync_SecondPage_ReturnsSecondPageItems()
    {
        // Arrange
        var query = _context.TestEntities.OrderBy(e => e.Id);
        var pageIndex = 1;
        var pageSize = 10;

        // Act
        var result = await query.PagingWrapAsync(pageIndex, pageSize);

        // Assert
        Assert.That(result.PageIndex, Is.EqualTo(1));
        Assert.That(result.Total, Is.EqualTo(100));
        var list = await result.List.ToListAsync();
        Assert.That(list.Count, Is.EqualTo(10));
        Assert.That(list.First().Id, Is.EqualTo(11));
        Assert.That(list.Last().Id, Is.EqualTo(20));
    }

    [Test]
    public async Task PagingWrapAsync_LastPage_ReturnsLastItems()
    {
        // Arrange
        var query = _context.TestEntities.OrderBy(e => e.Id);
        var pageIndex = 9;
        var pageSize = 10;

        // Act
        var result = await query.PagingWrapAsync(pageIndex, pageSize);

        // Assert
        Assert.That(result.PageIndex, Is.EqualTo(9));
        Assert.That(result.Total, Is.EqualTo(100));
        var list = await result.List.ToListAsync();
        Assert.That(list.Count, Is.EqualTo(10));
        Assert.That(list.First().Id, Is.EqualTo(91));
        Assert.That(list.Last().Id, Is.EqualTo(100));
    }

    [Test]
    public async Task PagingWrapAsync_PageIndexExceedsTotal_ClampsToLastPage()
    {
        // Arrange - 100 items, page size 10 = 10 pages (indices 0-9)
        var query = _context.TestEntities.OrderBy(e => e.Id);
        var pageIndex = 15;
        var pageSize = 10;

        // Act
        var result = await query.PagingWrapAsync(pageIndex, pageSize);

        // Assert
        Assert.That(result.PageIndex, Is.EqualTo(9));
        Assert.That(result.Total, Is.EqualTo(100));
    }

    [Test]
    public async Task PagingWrapAsync_NegativePageIndex_ClampsToZero()
    {
        // Arrange
        var query = _context.TestEntities.OrderBy(e => e.Id);
        var pageIndex = -5;
        var pageSize = 10;

        // Act
        var result = await query.PagingWrapAsync(pageIndex, pageSize);

        // Assert
        Assert.That(result.PageIndex, Is.EqualTo(0));
        var list = await result.List.ToListAsync();
        Assert.That(list.First().Id, Is.EqualTo(1));
    }

    [Test]
    public async Task PagingWrapAsync_EmptyTable_ReturnsEmptyResult()
    {
        // Arrange
        _context.TestEntities.RemoveRange(_context.TestEntities);
        await _context.SaveChangesAsync();
        var query = _context.TestEntities.OrderBy(e => e.Id);

        // Act
        var result = await query.PagingWrapAsync(0, 10);

        // Assert
        Assert.That(result.PageIndex, Is.EqualTo(0));
        Assert.That(result.Total, Is.EqualTo(0));
        var list = await result.List.ToListAsync();
        Assert.That(list.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task PagingWrapAsync_DefaultParameters_ReturnsFirstPageWithDefaultSize()
    {
        // Arrange
        var query = _context.TestEntities.OrderBy(e => e.Id);

        // Act - using defaults (pageIndex=0, pageSize=30)
        var result = await query.PagingWrapAsync();

        // Assert
        Assert.That(result.PageIndex, Is.EqualTo(0));
        Assert.That(result.Total, Is.EqualTo(100));
        var list = await result.List.ToListAsync();
        Assert.That(list.Count, Is.EqualTo(30));
    }

    [Test]
    public void PagingWrapAsync_NullQuery_ThrowsArgumentNullException()
    {
        // Arrange
        IQueryable<TestEntity>? query = null;

        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(async () => await query!.PagingWrapAsync(0, 10));
    }

    #endregion

    #region PagingWrapWithEnumerableListAsync Tests

    [Test]
    public async Task PagingWrapWithEnumerableListAsync_FirstPage_ReturnsMaterializedList()
    {
        // Arrange
        var query = _context.TestEntities.OrderBy(e => e.Id);
        var pageIndex = 0;
        var pageSize = 10;

        // Act
        var result = await query.PagingWrapWithEnumerableListAsync(pageIndex, pageSize);

        // Assert
        Assert.That(result.PageIndex, Is.EqualTo(0));
        Assert.That(result.Total, Is.EqualTo(100));
        Assert.That(result.List.Count(), Is.EqualTo(10));
        Assert.That(result.List.First().Id, Is.EqualTo(1));
        Assert.That(result.List.Last().Id, Is.EqualTo(10));
        Assert.That(result.List, Is.TypeOf<List<TestEntity>>());
    }

    [Test]
    public async Task PagingWrapWithEnumerableListAsync_SecondPage_ReturnsMaterializedSecondPage()
    {
        // Arrange
        var query = _context.TestEntities.OrderBy(e => e.Id);
        var pageIndex = 1;
        var pageSize = 10;

        // Act
        var result = await query.PagingWrapWithEnumerableListAsync(pageIndex, pageSize);

        // Assert
        Assert.That(result.PageIndex, Is.EqualTo(1));
        Assert.That(result.List.First().Id, Is.EqualTo(11));
        Assert.That(result.List.Last().Id, Is.EqualTo(20));
    }

    #endregion

    #region ToPagingWithEnumerableListAsync Tests

    [Test]
    public async Task ToPagingWithEnumerableListAsync_ConvertsQueryableToEnumerable()
    {
        // Arrange
        var query = _context.TestEntities.OrderBy(e => e.Id);
        var pagingQueryable = await query.PagingWrapAsync(0, 10);

        // Act
        var result = await pagingQueryable.ToPagingWithEnumerableListAsync();

        // Assert
        Assert.That(result.PageIndex, Is.EqualTo(0));
        Assert.That(result.Total, Is.EqualTo(100));
        Assert.That(result.List.Count(), Is.EqualTo(10));
        Assert.That(result.List, Is.TypeOf<List<TestEntity>>());
    }

    [Test]
    public async Task ToPagingWithEnumerableListAsync_PreservesPageIndex()
    {
        // Arrange
        var query = _context.TestEntities.OrderBy(e => e.Id);
        var pagingQueryable = await query.PagingWrapAsync(2, 10);

        // Act
        var result = await pagingQueryable.ToPagingWithEnumerableListAsync();

        // Assert
        Assert.That(result.PageIndex, Is.EqualTo(2));
        Assert.That(result.List.First().Id, Is.EqualTo(21));
        Assert.That(result.List.Last().Id, Is.EqualTo(30));
    }

    [Test]
    public void ToPagingWithEnumerableListAsync_NullPaging_ThrowsArgumentNullException()
    {
        // Arrange
        PagingWithQueryableList<TestEntity>? pagingQueryable = null;

        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(async () => await pagingQueryable!.ToPagingWithEnumerableListAsync());
    }

    #endregion

    #region CancellationToken Tests

    [Test]
    public async Task PagingWrapAsync_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        var query = _context.TestEntities.OrderBy(e => e.Id);
        var cts = new CancellationTokenSource();

        // Act
        var result = await query.PagingWrapAsync(0, 10, cancellationToken: cts.Token);

        // Assert
        Assert.That(result.PageIndex, Is.EqualTo(0));
        Assert.That(result.Total, Is.EqualTo(100));
    }

    [Test]
    public void PagingWrapAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        // Arrange
        var query = _context.TestEntities.OrderBy(e => e.Id);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        Assert.ThrowsAsync<OperationCanceledException>(async () => await query.PagingWrapAsync(0, 10, cancellationToken: cts.Token));
    }

    #endregion

    #region HasNextPage Tests (Normal Mode)

    [Test]
    public async Task PagingWrapAsync_NormalMode_FirstPage_HasNextPageTrue()
    {
        var query = _context.TestEntities.OrderBy(e => e.Id);
        var result = await query.PagingWrapAsync(0, 10);

        Assert.That(result.HasNextPage, Is.True);
    }

    [Test]
    public async Task PagingWrapAsync_NormalMode_LastPage_HasNextPageFalse()
    {
        var query = _context.TestEntities.OrderBy(e => e.Id);
        var result = await query.PagingWrapAsync(9, 10);

        Assert.That(result.HasNextPage, Is.False);
    }

    [Test]
    public async Task ToPagingWithEnumerableListAsync_PropagatesHasNextPage()
    {
        var query = _context.TestEntities.OrderBy(e => e.Id);
        var pagingQueryable = await query.PagingWrapAsync(0, 10);
        var result = await pagingQueryable.ToPagingWithEnumerableListAsync();

        Assert.That(result.HasNextPage, Is.True);
    }

    #endregion

    #region SkipCount Mode Tests

    [Test]
    public async Task PagingWrapAsync_SkipCount_FirstPage_HasNextPageTrue()
    {
        var query = _context.TestEntities.OrderBy(e => e.Id);
        var result = await query.PagingWrapAsync(0, 10, PagingCountMode.SkipCount);

        Assert.That(result.HasNextPage, Is.True);
        Assert.That(result.Total, Is.EqualTo(-1));
        var list = result.List.ToList();
        Assert.That(list.Count, Is.EqualTo(10));
        Assert.That(list.First().Id, Is.EqualTo(1));
        Assert.That(list.Last().Id, Is.EqualTo(10));
    }

    [Test]
    public async Task PagingWrapAsync_SkipCount_LastPage_HasNextPageFalse()
    {
        var query = _context.TestEntities.OrderBy(e => e.Id);
        var result = await query.PagingWrapAsync(9, 10, PagingCountMode.SkipCount);

        Assert.That(result.HasNextPage, Is.False);
        Assert.That(result.Total, Is.EqualTo(-1));
        var list = result.List.ToList();
        Assert.That(list.Count, Is.EqualTo(10));
        Assert.That(list.First().Id, Is.EqualTo(91));
        Assert.That(list.Last().Id, Is.EqualTo(100));
    }

    [Test]
    public async Task PagingWrapAsync_SkipCount_EmptyTable_HasNextPageFalse()
    {
        _context.TestEntities.RemoveRange(_context.TestEntities);
        await _context.SaveChangesAsync();
        var query = _context.TestEntities.OrderBy(e => e.Id);

        var result = await query.PagingWrapAsync(0, 10, PagingCountMode.SkipCount);

        Assert.That(result.HasNextPage, Is.False);
        Assert.That(result.Total, Is.EqualTo(-1));
        var list = result.List.ToList();
        Assert.That(list.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task PagingWrapAsync_SkipCount_NegativePageIndex_ClampsToZero()
    {
        var query = _context.TestEntities.OrderBy(e => e.Id);
        var result = await query.PagingWrapAsync(-5, 10, PagingCountMode.SkipCount);

        Assert.That(result.PageIndex, Is.EqualTo(0));
        var list = result.List.ToList();
        Assert.That(list.First().Id, Is.EqualTo(1));
    }

    [Test]
    public async Task PagingWrapWithEnumerableListAsync_SkipCount_FirstPage_HasNextPageTrue()
    {
        var query = _context.TestEntities.OrderBy(e => e.Id);
        var result = await query.PagingWrapWithEnumerableListAsync(0, 10, PagingCountMode.SkipCount);

        Assert.That(result.HasNextPage, Is.True);
        Assert.That(result.Total, Is.EqualTo(-1));
        Assert.That(result.List.Count(), Is.EqualTo(10));
        Assert.That(result.List, Is.TypeOf<List<TestEntity>>());
    }

    [Test]
    public async Task PagingWrapWithEnumerableListAsync_SkipCount_LastPage_HasNextPageFalse()
    {
        var query = _context.TestEntities.OrderBy(e => e.Id);
        var result = await query.PagingWrapWithEnumerableListAsync(9, 10, PagingCountMode.SkipCount);

        Assert.That(result.HasNextPage, Is.False);
        Assert.That(result.Total, Is.EqualTo(-1));
        Assert.That(result.List.Count(), Is.EqualTo(10));
    }

    [Test]
    public async Task PagingWrapAsync_SkipCount_WithCancellationToken_CompletesSuccessfully()
    {
        var query = _context.TestEntities.OrderBy(e => e.Id);
        var cts = new CancellationTokenSource();

        var result = await query.PagingWrapAsync(0, 10, PagingCountMode.SkipCount, cts.Token);

        Assert.That(result.HasNextPage, Is.True);
        Assert.That(result.Total, Is.EqualTo(-1));
    }

    #endregion

    #region Test Helpers

    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
        public DbSet<TestEntity> TestEntities => Set<TestEntity>();
    }

    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    #endregion
}

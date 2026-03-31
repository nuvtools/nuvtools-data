using NUnit.Framework;
using NuvTools.Data.Paging;
using NuvTools.Data.Paging.Enumerations;

namespace NuvTools.Data.Test.Paging;

[TestFixture]
public class PagingExtensionsTests
{
    private List<int> _testData = null!;

    [SetUp]
    public void Setup()
    {
        // Create test data: 1, 2, 3, ..., 100
        _testData = Enumerable.Range(1, 100).ToList();
    }

    #region PagingWrap Tests

    [Test]
    public void PagingWrap_FirstPage_ReturnsFirstItems()
    {
        // Arrange
        var pageIndex = 0;
        var pageSize = 10;

        // Act
        var result = _testData.PagingWrap(pageIndex, pageSize);

        // Assert
        Assert.That(result.PageIndex, Is.EqualTo(0));
        Assert.That(result.Total, Is.EqualTo(100));
        Assert.That(result.List.Count(), Is.EqualTo(10));
        Assert.That(result.List.First(), Is.EqualTo(1));
        Assert.That(result.List.Last(), Is.EqualTo(10));
    }

    [Test]
    public void PagingWrap_SecondPage_ReturnsSecondPageItems()
    {
        // Arrange
        var pageIndex = 1;
        var pageSize = 10;

        // Act
        var result = _testData.PagingWrap(pageIndex, pageSize);

        // Assert
        Assert.That(result.PageIndex, Is.EqualTo(1));
        Assert.That(result.Total, Is.EqualTo(100));
        Assert.That(result.List.Count(), Is.EqualTo(10));
        Assert.That(result.List.First(), Is.EqualTo(11));
        Assert.That(result.List.Last(), Is.EqualTo(20));
    }

    [Test]
    public void PagingWrap_LastPage_ReturnsRemainingItems()
    {
        // Arrange - 95 items, page size 10, last page has 5 items
        var data = Enumerable.Range(1, 95).ToList();
        var pageIndex = 9;
        var pageSize = 10;

        // Act
        var result = data.PagingWrap(pageIndex, pageSize);

        // Assert
        Assert.That(result.PageIndex, Is.EqualTo(9));
        Assert.That(result.Total, Is.EqualTo(95));
        Assert.That(result.List.Count(), Is.EqualTo(5));
        Assert.That(result.List.First(), Is.EqualTo(91));
        Assert.That(result.List.Last(), Is.EqualTo(95));
    }

    [Test]
    public void PagingWrap_PageIndexExceedsTotal_ClampsToLastPage()
    {
        // Arrange - 50 items, page size 10 = 5 pages (indices 0-4)
        var data = Enumerable.Range(1, 50).ToList();
        var pageIndex = 10;
        var pageSize = 10;

        // Act
        var result = data.PagingWrap(pageIndex, pageSize);

        // Assert
        Assert.That(result.PageIndex, Is.EqualTo(4));
        Assert.That(result.Total, Is.EqualTo(50));
        Assert.That(result.List.Count(), Is.EqualTo(10));
        Assert.That(result.List.First(), Is.EqualTo(41));
        Assert.That(result.List.Last(), Is.EqualTo(50));
    }

    [Test]
    public void PagingWrap_NegativePageIndex_ClampsToZero()
    {
        // Arrange
        var pageIndex = -5;
        var pageSize = 10;

        // Act
        var result = _testData.PagingWrap(pageIndex, pageSize);

        // Assert
        Assert.That(result.PageIndex, Is.EqualTo(0));
        Assert.That(result.Total, Is.EqualTo(100));
        Assert.That(result.List.Count(), Is.EqualTo(10));
        Assert.That(result.List.First(), Is.EqualTo(1));
        Assert.That(result.List.Last(), Is.EqualTo(10));
    }

    [Test]
    public void PagingWrap_EmptyCollection_ReturnsEmptyResult()
    {
        // Arrange
        var data = new List<int>();
        var pageIndex = 0;
        var pageSize = 10;

        // Act
        var result = data.PagingWrap(pageIndex, pageSize);

        // Assert
        Assert.That(result.PageIndex, Is.EqualTo(0));
        Assert.That(result.Total, Is.EqualTo(0));
        Assert.That(result.List.Count(), Is.EqualTo(0));
    }

    [Test]
    public void PagingWrap_DefaultParameters_ReturnsFirstPageWithDefaultSize()
    {
        // Act - using defaults (pageIndex=0, pageSize=50)
        var result = _testData.PagingWrap();

        // Assert
        Assert.That(result.PageIndex, Is.EqualTo(0));
        Assert.That(result.Total, Is.EqualTo(100));
        Assert.That(result.List.Count(), Is.EqualTo(50));
        Assert.That(result.List.First(), Is.EqualTo(1));
        Assert.That(result.List.Last(), Is.EqualTo(50));
    }

    #endregion

    #region Paging on IEnumerable Tests

    [Test]
    public void Paging_IEnumerable_FirstPage_ReturnsFirstItems()
    {
        // Arrange
        var pageIndex = 0;
        var pageSize = 10;

        // Act
        var result = _testData.Paging(pageIndex, pageSize).ToList();

        // Assert
        Assert.That(result.Count, Is.EqualTo(10));
        Assert.That(result.First(), Is.EqualTo(1));
        Assert.That(result.Last(), Is.EqualTo(10));
    }

    [Test]
    public void Paging_IEnumerable_SecondPage_ReturnsSecondPageItems()
    {
        // Arrange
        var pageIndex = 1;
        var pageSize = 10;

        // Act
        var result = _testData.Paging(pageIndex, pageSize).ToList();

        // Assert
        Assert.That(result.Count, Is.EqualTo(10));
        Assert.That(result.First(), Is.EqualTo(11));
        Assert.That(result.Last(), Is.EqualTo(20));
    }

    [Test]
    public void Paging_IEnumerable_NegativeIndex_ReturnsFirstPage()
    {
        // Arrange
        var pageIndex = -1;
        var pageSize = 10;

        // Act
        var result = _testData.Paging(pageIndex, pageSize).ToList();

        // Assert
        Assert.That(result.Count, Is.EqualTo(10));
        Assert.That(result.First(), Is.EqualTo(1));
        Assert.That(result.Last(), Is.EqualTo(10));
    }

    #endregion

    #region Paging on IQueryable Tests

    [Test]
    public void Paging_IQueryable_FirstPage_ReturnsFirstItems()
    {
        // Arrange
        var queryable = _testData.AsQueryable();
        var pageIndex = 0;
        var pageSize = 10;

        // Act
        var result = queryable.Paging(pageIndex, pageSize).ToList();

        // Assert
        Assert.That(result.Count, Is.EqualTo(10));
        Assert.That(result.First(), Is.EqualTo(1));
        Assert.That(result.Last(), Is.EqualTo(10));
    }

    [Test]
    public void Paging_IQueryable_SecondPage_ReturnsSecondPageItems()
    {
        // Arrange
        var queryable = _testData.AsQueryable();
        var pageIndex = 1;
        var pageSize = 10;

        // Act
        var result = queryable.Paging(pageIndex, pageSize).ToList();

        // Assert
        Assert.That(result.Count, Is.EqualTo(10));
        Assert.That(result.First(), Is.EqualTo(11));
        Assert.That(result.Last(), Is.EqualTo(20));
    }

    [Test]
    public void Paging_IQueryable_ThirdPage_ReturnsThirdPageItems()
    {
        // Arrange
        var queryable = _testData.AsQueryable();
        var pageIndex = 2;
        var pageSize = 10;

        // Act
        var result = queryable.Paging(pageIndex, pageSize).ToList();

        // Assert
        Assert.That(result.Count, Is.EqualTo(10));
        Assert.That(result.First(), Is.EqualTo(21));
        Assert.That(result.Last(), Is.EqualTo(30));
    }

    #endregion

    #region ToPagingEnumerable Tests

    [Test]
    public void ToPagingEnumerable_ConvertsQueryableToEnumerable()
    {
        // Arrange
        var queryable = _testData.AsQueryable();
        var pagingQueryable = new PagingWithQueryableList<int>
        {
            List = queryable.Take(10),
            PageIndex = 0,
            Total = 100
        };

        // Act
        var result = pagingQueryable.ToPagingEnumerable();

        // Assert
        Assert.That(result.PageIndex, Is.EqualTo(0));
        Assert.That(result.Total, Is.EqualTo(100));
        Assert.That(result.List.Count(), Is.EqualTo(10));
        Assert.That(result.List, Is.InstanceOf<IEnumerable<int>>());
    }

    [Test]
    public void ToPagingEnumerable_PreservesPageIndex()
    {
        // Arrange
        var queryable = _testData.AsQueryable();
        var pagingQueryable = new PagingWithQueryableList<int>
        {
            List = queryable.Skip(20).Take(10),
            PageIndex = 2,
            Total = 100
        };

        // Act
        var result = pagingQueryable.ToPagingEnumerable();

        // Assert
        Assert.That(result.PageIndex, Is.EqualTo(2));
        Assert.That(result.List.First(), Is.EqualTo(21));
        Assert.That(result.List.Last(), Is.EqualTo(30));
    }

    [Test]
    public void ToPagingEnumerable_NullPaging_ThrowsArgumentNullException()
    {
        // Arrange
        PagingWithQueryableList<int>? pagingQueryable = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => pagingQueryable!.ToPagingEnumerable());
    }

    [Test]
    public void ToPagingEnumerable_PropagatesHasNextPage()
    {
        // Arrange
        var queryable = _testData.AsQueryable();
        var pagingQueryable = new PagingWithQueryableList<int>
        {
            List = queryable.Take(10),
            PageIndex = 0,
            Total = 100,
            HasNextPage = true
        };

        // Act
        var result = pagingQueryable.ToPagingEnumerable();

        // Assert
        Assert.That(result.HasNextPage, Is.True);
    }

    #endregion

    #region HasNextPage Tests (Normal Mode)

    [Test]
    public void PagingWrap_NormalMode_FirstPageOfMultiple_HasNextPageTrue()
    {
        var result = _testData.PagingWrap(0, 10);
        Assert.That(result.HasNextPage, Is.True);
    }

    [Test]
    public void PagingWrap_NormalMode_LastPage_HasNextPageFalse()
    {
        var result = _testData.PagingWrap(9, 10);
        Assert.That(result.HasNextPage, Is.False);
    }

    [Test]
    public void PagingWrap_NormalMode_SinglePage_HasNextPageFalse()
    {
        var data = Enumerable.Range(1, 5).ToList();
        var result = data.PagingWrap(0, 10);
        Assert.That(result.HasNextPage, Is.False);
    }

    #endregion

    #region SkipCount Mode Tests

    [Test]
    public void PagingWrap_SkipCount_FirstPageWithMoreData_HasNextPageTrue()
    {
        var result = _testData.PagingWrap(0, 10, PagingCountMode.SkipCount);

        Assert.That(result.HasNextPage, Is.True);
        Assert.That(result.Total, Is.EqualTo(-1));
        Assert.That(result.List.Count(), Is.EqualTo(10));
        Assert.That(result.List.First(), Is.EqualTo(1));
        Assert.That(result.List.Last(), Is.EqualTo(10));
    }

    [Test]
    public void PagingWrap_SkipCount_LastPage_HasNextPageFalse()
    {
        // 100 items, page 9, size 10 => items 91-100, no next page
        var result = _testData.PagingWrap(9, 10, PagingCountMode.SkipCount);

        Assert.That(result.HasNextPage, Is.False);
        Assert.That(result.Total, Is.EqualTo(-1));
        Assert.That(result.List.Count(), Is.EqualTo(10));
        Assert.That(result.List.First(), Is.EqualTo(91));
        Assert.That(result.List.Last(), Is.EqualTo(100));
    }

    [Test]
    public void PagingWrap_SkipCount_PartialLastPage_HasNextPageFalse()
    {
        var data = Enumerable.Range(1, 95).ToList();
        var result = data.PagingWrap(9, 10, PagingCountMode.SkipCount);

        Assert.That(result.HasNextPage, Is.False);
        Assert.That(result.Total, Is.EqualTo(-1));
        Assert.That(result.List.Count(), Is.EqualTo(5));
    }

    [Test]
    public void PagingWrap_SkipCount_EmptyCollection_HasNextPageFalse()
    {
        var data = new List<int>();
        var result = data.PagingWrap(0, 10, PagingCountMode.SkipCount);

        Assert.That(result.HasNextPage, Is.False);
        Assert.That(result.Total, Is.EqualTo(-1));
        Assert.That(result.List.Count(), Is.EqualTo(0));
    }

    [Test]
    public void PagingWrap_SkipCount_NegativePageIndex_ClampsToZero()
    {
        var result = _testData.PagingWrap(-5, 10, PagingCountMode.SkipCount);

        Assert.That(result.PageIndex, Is.EqualTo(0));
        Assert.That(result.List.First(), Is.EqualTo(1));
    }

    [Test]
    public void PagingWrap_SkipCount_BeyondData_ReturnsEmpty()
    {
        // Request page 100 of 100-item dataset with size 10 => no items
        var result = _testData.PagingWrap(100, 10, PagingCountMode.SkipCount);

        Assert.That(result.HasNextPage, Is.False);
        Assert.That(result.List.Count(), Is.EqualTo(0));
    }

    #endregion
}

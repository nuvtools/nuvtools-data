using NUnit.Framework;
using NuvTools.Data.Paging;

namespace NuvTools.Data.Test.Paging;

[TestFixture]
public class PagingHelperTests
{
    #region GetPageIndex Tests

    [Test]
    public void GetPageIndex_ValidIndex_ReturnsRequestedIndex()
    {
        // Arrange
        var index = 2;
        var pageSize = 10;
        var total = 100;

        // Act
        var result = PagingHelper.GetPageIndex(index, pageSize, total);

        // Assert
        Assert.That(result, Is.EqualTo(2));
    }

    [Test]
    public void GetPageIndex_FirstPage_ReturnsZero()
    {
        // Arrange
        var index = 0;
        var pageSize = 10;
        var total = 100;

        // Act
        var result = PagingHelper.GetPageIndex(index, pageSize, total);

        // Assert
        Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public void GetPageIndex_LastPage_ReturnsCorrectIndex()
    {
        // Arrange - 100 items, 10 per page = 10 pages (indices 0-9)
        var index = 9;
        var pageSize = 10;
        var total = 100;

        // Act
        var result = PagingHelper.GetPageIndex(index, pageSize, total);

        // Assert
        Assert.That(result, Is.EqualTo(9));
    }

    [Test]
    public void GetPageIndex_IndexExceedsTotal_ClampsToLastValidIndex()
    {
        // Arrange - 50 items, 10 per page = 5 pages (indices 0-4)
        var index = 10;
        var pageSize = 10;
        var total = 50;

        // Act
        var result = PagingHelper.GetPageIndex(index, pageSize, total);

        // Assert
        Assert.That(result, Is.EqualTo(4));
    }

    [Test]
    public void GetPageIndex_NegativeIndex_ClampsToZero()
    {
        // Arrange
        var index = -5;
        var pageSize = 10;
        var total = 100;

        // Act
        var result = PagingHelper.GetPageIndex(index, pageSize, total);

        // Assert
        Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public void GetPageIndex_EmptyCollection_ReturnsZero()
    {
        // Arrange
        var index = 5;
        var pageSize = 10;
        var total = 0;

        // Act
        var result = PagingHelper.GetPageIndex(index, pageSize, total);

        // Assert
        Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public void GetPageIndex_ZeroPageSize_ReturnsZero()
    {
        // Arrange
        var index = 2;
        var pageSize = 0;
        var total = 100;

        // Act
        var result = PagingHelper.GetPageIndex(index, pageSize, total);

        // Assert
        Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public void GetPageIndex_NegativeTotal_ReturnsZero()
    {
        // Arrange
        var index = 2;
        var pageSize = 10;
        var total = -10;

        // Act
        var result = PagingHelper.GetPageIndex(index, pageSize, total);

        // Assert
        Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public void GetPageIndex_PartialLastPage_CalculatesCorrectly()
    {
        // Arrange - 95 items, 10 per page = 10 pages (indices 0-9, last page has 5 items)
        var index = 9;
        var pageSize = 10;
        var total = 95;

        // Act
        var result = PagingHelper.GetPageIndex(index, pageSize, total);

        // Assert
        Assert.That(result, Is.EqualTo(9));
    }

    [Test]
    public void GetPageIndex_SingleItemCollection_ReturnsZero()
    {
        // Arrange - 1 item, 10 per page = 1 page (index 0)
        var index = 0;
        var pageSize = 10;
        var total = 1;

        // Act
        var result = PagingHelper.GetPageIndex(index, pageSize, total);

        // Assert
        Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public void GetPageIndex_SingleItemCollectionRequestingPage1_ClampsToZero()
    {
        // Arrange - 1 item, 10 per page = 1 page (index 0)
        var index = 1;
        var pageSize = 10;
        var total = 1;

        // Act
        var result = PagingHelper.GetPageIndex(index, pageSize, total);

        // Assert
        Assert.That(result, Is.EqualTo(0));
    }

    #endregion
}

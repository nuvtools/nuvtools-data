namespace NuvTools.Data.Paging;

/// <summary>
/// Helper methods for paging calculations.
/// </summary>
public static class PagingHelper
{
    /// <summary>
    /// Calculates the valid page index, ensuring it doesn't exceed the total number of pages.
    /// </summary>
    /// <param name="index">The requested page index (0-indexed).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="total">The total number of items.</param>
    /// <returns>A valid page index (0-indexed) that doesn't exceed the total pages available.</returns>
    public static int GetPageIndex(int index, int pageSize, int total)
    {
        if (total <= 0 || pageSize <= 0) return 0;

        var totalPages = (int)Math.Ceiling(total / (decimal)pageSize);
        var maxIndex = totalPages - 1;

        if (index < 0) return 0;
        if (index > maxIndex) return maxIndex;

        return index;
    }

    /// <summary>
    /// Determines whether there is a next page based on the current page index, page size, and total count.
    /// </summary>
    /// <param name="pageIndex">The current page index (0-indexed).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="total">The total number of items.</param>
    /// <returns>True if there are more items beyond the current page; otherwise, false.</returns>
    public static bool CalculateHasNextPage(int pageIndex, int pageSize, int total)
    {
        if (total <= 0 || pageSize <= 0) return false;
        return (pageIndex + 1) * pageSize < total;
    }

    /// <summary>
    /// Calculates the number of items to skip for pagination.
    /// </summary>
    /// <param name="index">The page index (0-indexed).</param>
    /// <param name="size">The page size.</param>
    /// <returns>The number of items to skip.</returns>
    public static int GetSkip(int index, int size)
    {
        if (index < 0) index = 0;
        return index * size;
    }

}

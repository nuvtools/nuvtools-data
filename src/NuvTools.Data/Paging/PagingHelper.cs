namespace NuvTools.Data.Paging;

/// <summary>
/// Helper methods for paging calculations.
/// </summary>
public static class PagingHelper
{
    /// <summary>
    /// Calculates the valid page index, ensuring it doesn't exceed the total number of pages.
    /// </summary>
    /// <param name="pageIndex">The requested page index (0-indexed).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="total">The total number of items.</param>
    /// <returns>A valid page index that doesn't exceed the total pages available.</returns>
    public static int GetPageIndex(int pageIndex, int pageSize, int total)
    {
        var totalPages = total > 0 ? (int)Math.Ceiling(total / (decimal)pageSize) : 0;
        var maxIndex = totalPages > 0 ? totalPages - 1 : 0;
        return pageIndex < 0 ? 0 : (pageIndex > maxIndex ? maxIndex : pageIndex);
    }

    /// <summary>
    /// Calculates the number of items to skip for pagination.
    /// </summary>
    /// <param name="pageIndex">The page index (0-indexed).</param>
    /// <param name="size">The page size.</param>
    /// <returns>The number of items to skip.</returns>
    public static int GetSkip(int pageIndex, int size)
    {
        if (pageIndex < 0) pageIndex = 0;
        return pageIndex * size;
    }

    /// <summary>
    /// Gets the page index when the total is unknown.
    /// Ensures the page index is at least 0.
    /// </summary>
    /// <param name="pageIndex">The requested page index (0-indexed).</param>
    /// <returns>A valid page index (at least 0).</returns>
    public static int GetPageIndexWithoutTotal(int pageIndex)
    {
        return pageIndex < 0 ? 0 : pageIndex;
    }

    /// <summary>
    /// Calculates the total number of pages for a given total count and page size.
    /// </summary>
    /// <param name="total">The total number of items. Can be null if unknown.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>The total number of pages, or null if the total is unknown.</returns>
    public static int? GetTotalPages(int? total, int pageSize)
    {
        if (!total.HasValue || pageSize <= 0)
            return null;

        return total.Value > 0 ? (int)Math.Ceiling(total.Value / (decimal)pageSize) : 0;
    }

}

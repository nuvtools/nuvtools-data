namespace NuvTools.Data.Paging;

/// <summary>
/// Helper methods for paging calculations.
/// </summary>
public static class PagingHelper
{
    /// <summary>
    /// Calculates the valid page number, ensuring it doesn't exceed the total number of pages.
    /// </summary>
    /// <param name="index">The requested page number (1-indexed).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="total">The total number of items.</param>
    /// <returns>A valid page number that doesn't exceed the total pages available.</returns>
    public static int GetPageNumber(int index, int pageSize, int total)
    {
        var totalPage = total > 0 ? (int)Math.Ceiling(total / (decimal)pageSize) : 0;
        return (totalPage - index) < 0 ? totalPage : index;
    }

    /// <summary>
    /// Calculates the number of items to skip for pagination.
    /// </summary>
    /// <param name="index">The page number (1-indexed).</param>
    /// <param name="size">The page size.</param>
    /// <returns>The number of items to skip.</returns>
    public static int GetSkip(int index, int size)
    {
        if (index < 1) index = 1;
        return (index - 1) * size;
    }

    /// <summary>
    /// Gets the page number when the total is unknown.
    /// Ensures the page number is at least 1.
    /// </summary>
    /// <param name="index">The requested page number (1-indexed).</param>
    /// <returns>A valid page number (at least 1).</returns>
    public static int GetPageNumberWithoutTotal(int index)
    {
        return index < 1 ? 1 : index;
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

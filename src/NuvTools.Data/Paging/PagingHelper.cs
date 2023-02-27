namespace NuvTools.Data.Paging;

public static class PagingHelper
{
    public static int GetPageNumber(int index, int pageSize, int total)
    {
        var totalPage = total > 0 ? (int)Math.Ceiling(total / (decimal)pageSize) : 0;
        return (totalPage - index) < 0 ? totalPage : index;
    }

    internal static int GetSkip(int index, int size)
    {
        if (index < 1) index = 1;
        return (index - 1) * size;
    }

}

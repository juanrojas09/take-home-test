
namespace Fundo.Applications.Domain.Common;

public class Pagination<T>
{
    public List<T> Items { get; set; }
    public int TotalCount { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public double TotalPages => CalculateTotalPages(TotalCount, PageSize);
    
    public static Pagination<T> Create(List<T> items, int totalCount, int currentPage, int pageSize)
    {
        return new Pagination<T>
        {
            Items = items,
            TotalCount = totalCount,
            CurrentPage = currentPage,
            PageSize = pageSize
        };
    }

    public static int CalculateOffset(int page, int pageSize)
    {
        var offset=(page-1)*pageSize;
        return offset<0 ? 0:offset;
    } 


    public static double CalculateTotalPages(int totalCount, int pageSize)
    {
        if (pageSize <= 0) return 0;
        return Math.Ceiling((double)(totalCount / pageSize));
    }

}
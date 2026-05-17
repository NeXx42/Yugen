namespace Yugen.Domain.Data;

public class PageResponse<T>
{
    public int page { get; set; }
    public int pageSize { get; set; }
    public int totalResults { get; set; }

    public T[] data { get; set; }

    public PageResponse(T[] data, int page, int pageSize, int count)
    {
        this.data = data;
        this.page = page;
        this.pageSize = pageSize;
        this.totalResults = count;
    }

    public static PageResponse<T> Empty()
    {
        return new PageResponse<T>(Array.Empty<T>(), 0, 0, 0);
    }
}

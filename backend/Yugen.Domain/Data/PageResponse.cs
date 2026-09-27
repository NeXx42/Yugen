namespace Yugen.Domain.Data;

public class PageResponse<T>
{
    public int page { get; set; }
    public int pageSize { get; set; }
    public int totalResults { get; set; }

    public T[] data { get; set; }

    public PageResponse(IEnumerable<int> ordering, IEnumerable<T> data, Func<T, int> getKey, int page, int pageSize, int count)
    {
        List<T> orderedData = new(Math.Min(ordering.Count(), data.Count()));
        Dictionary<int, T> keyedData = data.ToDictionary(d => getKey(d), d => d);

        foreach (int orderedKey in ordering)
            if (keyedData.TryGetValue(orderedKey, out T? dat))
                orderedData.Add(dat);

        this.data = orderedData.ToArray();
        this.page = page;
        this.pageSize = pageSize;
        this.totalResults = count;
    }

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

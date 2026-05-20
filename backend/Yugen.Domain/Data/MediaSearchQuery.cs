using Yugen.Domain.Enums;

namespace Yugen.Domain.Data;

public class MediaSearchQuery
{
    public int? page { get; set; }
    public int? pageSize { get; set; }

    public string? text { get; set; }
    public MediaSort? sort { get; set; }

    public string GetCacheKey() => Guid.NewGuid().ToString(); // temp
}

using Yugen.Domain.Enums;

namespace Yugen.Domain.Data;

public class MediaSearchQuery
{
    public int? page { get; set; }
    public int? pageSize { get; set; }

    public string? text { get; set; }
    public MediaSort? sort { get; set; }

    public long? lesserStartDate { get; set; }
    public string? season { get; set; }
    public int? year { get; set; }

    public string GetCacheKey() => $"{page}_{pageSize}_{text}_{sort}_{lesserStartDate}_{season}_{year}";
}

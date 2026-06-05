using Yugen.Domain.Enums;

namespace Yugen.Domain.Data;

public class MediaSearchQuery
{
    public int? page { get; set; }
    public int? pageSize { get; set; }

    public ICollection<int>? ids { get; set; }
    public string? text { get; set; }
    public MediaSort? sort { get; set; }

    public bool? allowAdultContent { get; set; }
    public long? lesserStartDate { get; set; }
    public string? season { get; set; }
    public int? year { get; set; }
    public string? format { get; set; }
    public string? status { get; set; }

    public string[]? tags { get; set; }
    public string[]? genres { get; set; }

    public string GetCacheKey() => $"{page}_{pageSize}_{text}_{sort}_{allowAdultContent}_{lesserStartDate}_{season}_{year}_{format}_{status}_{tags?.GetHashCode()}_{genres?.GetHashCode()}";
}

namespace Yugen.Domain.Data.Downloads;

public class DownloadRequest
{
    public required int seriesId { get; set; }
    public required int seasonId { get; set; }

    public required string rootPath { get; set; }
    public required int qualityId { get; set; }

    public bool monitorSeason { get; set; }
}

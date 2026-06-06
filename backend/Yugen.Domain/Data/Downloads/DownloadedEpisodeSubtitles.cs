namespace Yugen.Domain.Data.Downloads;

public class DownloadedEpisodeSubtitles
{
    public required int subtitleId { get; set; }
    public required string jellyfinEpisodeId { get; set; }

    public string? languageCode { get; set; }
    public string? title { get; set; }
}

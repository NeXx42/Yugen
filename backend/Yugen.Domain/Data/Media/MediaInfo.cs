namespace Yugen.Domain.Data.Media;

public class MediaInfo
{
    public required string title { get; set; }
    public required bool isDownloaded { get; set; }

    public class Seasons
    {
        public string[]? episodeNames { get; set; }
    }
}

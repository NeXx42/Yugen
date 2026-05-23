namespace Yugen.Domain.Data.Downloads;

public class DownloadRequestInfo
{
    public bool monitored { get; set; }
    public int? sonarrRequestId { get; set; }
    public int? sonarrSeasonId { get; set; }

    public int? selectedRoot { get; set; }
    public int? selectedQuality { get; set; }

    public required Roots[] roots { get; set; }
    public required Qualities[] qualities { get; set; }

    public Episode?[]? downloadedEpisodes { get; set; }

    public class Episode
    {
        public required int providerId { get; set; }
        public required int episodeNumber { get; set; }
        public required bool monitored { get; set; }

        public string? jellyfinId { get; set; }
    }

    public class Roots
    {
        public required string path { get; set; }
        public long? freeSpace { get; set; }
    }

    public class Qualities
    {
        public required int id { get; set; }
        public string? title { get; set; }
    }
}


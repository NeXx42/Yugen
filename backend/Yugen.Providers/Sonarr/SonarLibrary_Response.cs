namespace Yugen.Providers.Sonarr;

public class SonarrLibrary_Response_Series
{
    public int id { get; set; }
    public string? path { get; set; }
    public string? title { get; set; }

    public required int qualityProfileId { get; set; }
    public required string rootFolderPath { get; set; }

    public int tvdbId { get; set; }
    public int tmdbId { get; set; }

    public bool monitored { get; set; }
    public bool seasonFolder { get; set; }

    public Seasons[]? seasons { get; set; }

    public class Seasons
    {
        public int seasonNumber { get; set; }
        public bool monitored { get; set; }
    }
}

public class SonarrLibrary_Response_Episode
{
    public int id { get; set; }

    public int seasonNumber { get; set; }
    public int episodeFileId { get; set; }
    public int episodeNumber { get; set; }

    public bool monitored { get; set; }
}

public class SonarrLibrary_Response_EpisodeFile
{
    public int id { get; set; }
    public int seasonNumber { get; set; }

    public string? relativePath { get; set; }
    public string? path { get; set; }

    public Language[]? languages { get; set; }

    public class Language
    {
        public string? name { get; set; }
    }
}

public class SonarrLibrary_Response_AddRequest
{
    public int id { get; set; }
    public string? path { get; set; }

    public int? tvdbId { get; set; }
    public int? tmdbId { get; set; }
}

public class SonarrLibrary_Response_Roots
{
    public required string path { get; set; }
    public long? freeSpace { get; set; }
}

public class SonarrLibrary_Response_Qualities
{
    public required int id { get; set; }
    public string? title { get; set; }
}


public class SonarrNotification_WebhookMessage
{
    public Series? series { get; set; }

    public string? eventType { get; set; }
    public string? instanceName { get; set; }

    public class Series
    {
        public int? id { get; set; }
        public string? title { get; set; }
        public string? titleSlug { get; set; }
        public string? path { get; set; }
        public int? tvdbId { get; set; }
        public int? tvMazeId { get; set; }
        public int? tmdbId { get; set; }
        public string? imdbId { get; set; }
        public string? type { get; set; }
        public int? year { get; set; }
    }
}


public enum SonarrWebhookEventType
{
    Test,
    Grab,
    Download,
    Rename,
    SeriesAdd,
    SeriesDelete,
    EpisodeFileDelete,
    Health,
    ApplicationUpdate,
    HealthRestored,
    ManualInteractionRequired,
}
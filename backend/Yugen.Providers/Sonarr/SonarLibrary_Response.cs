namespace Yugen.Providers.Sonarr;

public class SonarrLibrary_Response_Series
{
    public int id { get; set; }
    public string? path { get; set; }

    public int? tvdbId { get; set; }
    public int? tmdbId { get; set; }

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
namespace Yugen.Providers.Sonarr;

public class SonarrRequest_FetchLibrary
{
    public required string title { get; set; }
    public required int tvdbId { get; set; }

    public required int qualityProfileId { get; set; }
    public required string rootFolderPath { get; set; }
    public required bool seasonFolder { get; set; }
    public required bool monitored { get; set; }

    public Season[]? seasons { get; set; }
    public required AddOptions addOptions { get; set; }

    public class Season
    {
        public required int seasonNumber { get; set; }
        public required bool monitored { get; set; }
    }

    public class AddOptions
    {
        public required bool searchForMissingEpisodes { get; set; }
    }
}
namespace Yugen.Providers.Radarr;

public class RadarrLibrary_Request_Request
{
    public required int tmdbId { get; set; }
    public required int qualityProfileId { get; set; }
    public required string rootFolderPath { get; set; }

    public required AddOptions addOptions { get; set; }

    public class AddOptions
    {
        public required bool ignoreEpisodesWithFiles { get; set; }
        public required bool ignoreEpisodesWithoutFiles { get; set; }
        public required string monitor { get; set; }
        public required bool searchForMovie { get; set; }
        public required string addMethod { get; set; }
    }
}

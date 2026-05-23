namespace Yugen.Providers.Sonarr;

public class SonarrRequest_FetchLibrary : SonarrLibrary_Response_Series
{
    public AddOptions? addOptions { get; set; }

    public class AddOptions
    {
        public required bool searchForMissingEpisodes { get; set; }
    }
}
namespace Yugen.Providers.Radarr;

public class RadarrLibrary_Response_Movie
{
    public required int id { get; set; }
    public int? tmdbId { get; set; }
    public MoveFile? movieFile { get; set; }

    public string? rootFolderPath { get; set; }
    public int? qualityProfileId { get; set; }

    public bool monitored { get; set; }

    public class MoveFile
    {
        public required int id { get; set; }
        public string? path { get; set; }
        public long? size { get; set; }
    }
}


public class RadarrLibrary_Response_Lookup
{
    public required int id { get; set; }
    public string? title { get; set; }
    public string? originalTitle { get; set; }
}
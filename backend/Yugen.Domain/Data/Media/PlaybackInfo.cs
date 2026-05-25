namespace Yugen.Domain.Data.Media;

public class PlaybackInfo
{
    public long? historicalTicks { get; set; }

    public required string jellyfinId { get; set; }
    public required Source[] sources { get; set; }

    public class Source
    {
        public required string id { get; set; }
        public Subtitles[]? subs { get; set; }

        public class Subtitles
        {
            public required string language { get; set; }
            public required string uri { get; set; }
        }
    }
}

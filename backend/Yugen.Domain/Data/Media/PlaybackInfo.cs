namespace Yugen.Domain.Data.Media;

public class PlaybackInfo
{
    public long? historicalTicks { get; set; }
    public Segment[]? segments { get; set; }
    public double[]? chapters { get; set; }

    public required string jellyfinId { get; set; }
    public required Source[] sources { get; set; }

    public class Segment
    {
        public double start { get; set; }
        public double duration { get; set; }

        public Segment(long start, long end, double duration)
        {
            this.start = (start / duration) * 100;
            this.duration = ((end - start) / duration) * 100;
        }
    }

    public class Source
    {
        public required string id { get; set; }

        public Subtitles[]? subs { get; set; }
        public Audio[]? audio { get; set; }

        public class Subtitles
        {
            public string? title { get; set; }
            public bool isExternal { get; set; }
            public string? language { get; set; }
            public required string uri { get; set; }
            public required int id { get; set; }
        }

        public class Audio
        {
            public required int id { get; set; }
            public string? title { get; set; }
            public bool isDefault { get; set; }
        }
    }
}

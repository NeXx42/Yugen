namespace Yugen.Providers.AniList;

public class AniListResponse_Search
{
    public required Data data { get; set; }

    public class Data
    {
        public required Page page { get; set; }

        public class Page
        {
            public AniListResponse_Media[]? media { get; set; }
            public PageInfo? pageInfo { get; set; }

            public class PageInfo
            {
                public int total { get; set; }
            }
        }
    }
}

public class AniListResponse_Info
{
    public required Data data { get; set; }

    public class Data
    {
        public required AniListResponse_Media media { get; set; }
    }
}

public class AniListResponse_Media
{
    public int id { get; set; }
    public int? malId { get; set; }
    public int? episodes { get; set; }
    public string? siteUrl { get; set; }

    public Title? title { get; set; }
    public string? type { get; set; }
    public string? status { get; set; }
    public string? description { get; set; }
    public string? format { get; set; }

    public int? averageScore { get; set; }
    public int? meanScore { get; set; }
    public int? popularity { get; set; }

    public int? duration { get; set; }
    public int? seasonYear { get; set; }
    public string? season { get; set; }
    public Date? startDate { get; set; }
    public Date? endDate { get; set; }

    public string? bannerImage { get; set; }
    public CoverImage? coverImage { get; set; }
    public Trailer? trailer { get; set; }

    public StreamingEpisode[]? streamingEpisodes { get; set; }

    public Tag[]? tags { get; set; }
    public string[]? genres { get; set; }

    public NodeList? recommendations { get; set; }
    public NextAiringEpisode? nextAiringEpisode { get; set; }

    public class Title
    {
        public string? romaji { get; set; }
        public string? english { get; set; }
        public string? native { get; set; }

        public string getBestMatch => (string.IsNullOrEmpty(english) ? native : english) ?? "ERROR";
    }

    public class CoverImage
    {
        public string? extraLarge { get; set; }
        public string? Large { get; set; }
        public string? medium { get; set; }
        public string? color { get; set; }
    }

    public class Tag
    {
        public required int id { get; set; }
    }

    public class NodeList
    {
        public Nodes[]? nodes { get; set; }

        public class Nodes
        {
            public required MediaRecommendation mediaRecommendation { get; set; }

            public class MediaRecommendation
            {
                public required int id { get; set; }
            }
        }
    }

    public class Trailer
    {
        public string? thumbnail { get; set; }
    }

    public class StreamingEpisode
    {
        public string? title { get; set; }
        public string? thumbnail { get; set; }
    }

    public class Date
    {
        public int? day { get; set; }
        public int? month { get; set; }
        public int? year { get; set; }

        public long? ToUnix() => (day.HasValue && month.HasValue && year.HasValue) ? new DateTimeOffset(year.Value, month.Value, day.Value, 0, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds() : null;
    }

    public class NextAiringEpisode
    {
        public long? airingAt { get; set; }
    }
}

public class AniListResponse_Airing
{
    public required Data data { get; set; }

    public class Data
    {
        public required Page page { get; set; }

        public class Page
        {
            public AiringSchedule[]? airingSchedules { get; set; }

            public class AiringSchedule
            {
                public int mediaId { get; set; }
                public long timeUntilAiring { get; set; }
            }
        }
    }
}


public class AniListResponse_AiringEpisode
{
    public Data? data { get; set; }

    public class Data
    {
        public Media? media { get; set; }

        public class Media
        {
            public NextAiringEpisode? nextAiringEpisode { get; set; }

            public class NextAiringEpisode
            {
                public long? airingAt { get; set; }
            }
        }
    }
}

public class AniListResponse_Trending
{
    public required Data data { get; set; }

    public class Data
    {
        public required Trending trending { get; set; }

        public class Trending
        {
            public required Media[] media { get; set; }

            public class Media
            {
                public required int id { get; set; }
            }
        }
    }
}

public class AniListResponse_Criteria
{
    public required Data data { get; set; }

    public class Data
    {
        public required string[] genreCollection { get; set; }
        public required MediaTag[] mediaTagCollection { get; set; }

        public class MediaTag
        {
            public required int id { get; set; }

            public bool isAdult { get; set; }
            public bool isGeneralSpoiler { get; set; }
            public bool isMediaSpoiler { get; set; }

            public string? name { get; set; }
            public string? category { get; set; }
            public string? description { get; set; }
        }
    }
}
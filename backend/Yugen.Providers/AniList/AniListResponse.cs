namespace Yugen.Providers.AniList;

public class AniListResponse_Search
{
    public required Data data { get; set; }

    public class Data
    {
        public required Page page { get; set; }

        public class Page
        {
            public Media[]? media { get; set; }

            public class Media
            {
                public string type { get; set; }

                public int id { get; set; }
                public int? malId { get; set; }
                public int? episodes { get; set; }

                public Title? title { get; set; }
                public CoverImage? coverImage { get; set; }

                public string? bannerImage { get; set; }

                public class Title
                {
                    public string? romaji { get; set; }
                    public string? english { get; set; }
                    public string? native { get; set; }

                    public string getBestMatch => string.IsNullOrEmpty(english) ? native : english;
                }

                public class CoverImage
                {
                    public string? extraLarge { get; set; }
                    public string? Large { get; set; }
                    public string? medium { get; set; }
                    public string? color { get; set; }
                }
            }
        }
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


public class AniListResponse_Info
{
    public class Data
    {
        public class Media
        {

        }
    }
}
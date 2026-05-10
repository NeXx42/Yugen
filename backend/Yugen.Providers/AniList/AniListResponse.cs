namespace Yugen.Providers.AniList;

public class AniListResponse_Search
{
    public required Data data { get; set; }

    public class Data
    {
        public required Page page { get; set; }

        public class Page
        {
            public MediaCard[]? media { get; set; }

            public class MediaCard
            {
                public int id { get; set; }
                public Title? title { get; set; }

                public class Title
                {
                    public string? romaji { get; set; }
                    public string? english { get; set; }
                    public string? native { get; set; }
                }
            }
        }
    }
}


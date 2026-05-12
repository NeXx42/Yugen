namespace Yugen.Domain.Data.Media;

public class ExternalMedia
{
    public Guid? jellyfinId { get; set; }
    public string? title { get; set; }

    public Season[]? seasons { get; set; }

    public class Season
    {
        public Guid jellyfinId { get; set; }

        public int number { get; set; }
        public string title { get; set; }
        public string aniListId { get; set; }

        public Episode[] episodes { get; set; }

        public class Episode
        {
            public Guid jellyfinId { get; set; }
            public int number { get; set; }
            public string title { get; set; }
        }
    }
}

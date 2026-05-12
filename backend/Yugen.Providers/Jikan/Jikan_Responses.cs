namespace Yugen.Providers.Jikan;


public class JikanResponses_Page
{
    public int last_visible_page { get; set; }
    public bool has_next_page { get; set; }
}

public class JikanReponse_Episodes
{
    public Episode[] data { get; set; }
    public JikanResponses_Page? page { get; set; }

    public class Episode
    {
        public int mal_id { get; set; }
        public string? url { get; set; }
        public string? title { get; set; }
        public string? title_japanese { get; set; }
        public string? title_romaji { get; set; }
        public string? aired { get; set; }
        public float? score { get; set; }
        public bool filler { get; set; }
        public bool recap { get; set; }
        public string? form_url { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;
using Yugen.Domain.Models.Media;

namespace Yugen.Domain.Models.Linking;

public class Model_Link
{
    [Required]
    [Key]
    public required int? anilist_id { get; set; }

    public string? type { get; set; }
    public int? anidb_id { get; set; }
    public int? animecountdown_id { get; set; }
    public int? animenewsnetwork_id { get; set; }
    public string? anime_planet_id { get; set; }
    public int? anisearch_id { get; set; }
    public string? imdb_id { get; set; }
    public int? kitsu_id { get; set; }
    public int? livechart_id { get; set; }
    public int? mal_id { get; set; }
    public int? simkl_id { get; set; }
    public int? themoviedb_id { get; set; }
    public int? tvdb_id { get; set; }

    public int? tvdb_season { get; set; }
    public int? tmdb_season { get; set; }
}

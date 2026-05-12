using System.ComponentModel.DataAnnotations;

namespace Yugen.Domain.Models.Media;

public class Model_MediaEpisode
{
    [Required]
    public int EpisodeNumber { get; set; }

    [Required]
    public int MediaId { get; set; }
    public Model_Media Media { get; set; } = null!;

    public string? EpisodeTitle { get; set; }
    public bool IsRecap { get; set; }
    public bool IsFiller { get; set; }
    public float? Score { get; set; }

    //public string? JellyfinItemId { get; set; }
}

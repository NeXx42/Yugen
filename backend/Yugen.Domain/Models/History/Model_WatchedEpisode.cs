using System.ComponentModel.DataAnnotations;

namespace Yugen.Domain.Models.History;

public class Model_WatchedEpisode
{
    [Required]
    public int HistoryId { get; set; }
    public Model_WatchHistory History { get; set; } = null!;

    [Required]
    public int EpisodeNumber { get; set; }

    public long? PlaybackPositionTicks { get; set; }
    public float? WatchPercentage { get; set; }
}

namespace Yugen.Domain.Models.History;

public class Model_WatchedEpisode
{
    public int MediaId { get; set; }
    public Model_WatchHistory WatchedHistory { get; set; } = null!;

    public int EpisodeNumber { get; set; }

    public long? PlaybackPositionTicks { get; set; }
    public float? WatchPercentage { get; set; }
    public DateTime? LastWatched { get; set; }
}

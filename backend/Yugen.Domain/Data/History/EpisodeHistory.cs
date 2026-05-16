using Yugen.Domain.Models.History;

namespace Yugen.Domain.Data.History;

public class WatchHistoryContainer
{
    public int? lastWatchedEpisode { get; set; }
    public EpisodeHistory[]? episodes { get; set; }
}

public class EpisodeHistory
{
    public int episode { get; set; }
    public float? watchPercentage { get; set; }

    public static EpisodeHistory Map(Model_WatchedEpisode db)
    {
        return new EpisodeHistory()
        {
            episode = db.EpisodeNumber,
            watchPercentage = db.WatchPercentage
        };
    }
}

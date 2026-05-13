using Yugen.Domain.Models.Library;

namespace Yugen.Domain.Data.Downloads;

public class DownloadedEpisode
{
    public int episode { get; set; }
    public string? jellyfinId { get; set; }

    public static DownloadedEpisode Map(Model_DownloadedEpisode db)
    {
        return new DownloadedEpisode()
        {
            episode = db.EpisodeNumber,
            jellyfinId = db.JellyfinId,
        };
    }
}

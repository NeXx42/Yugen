using Yugen.Domain.Models.History;
using Yugen.Domain.Models.Library;
using Yugen.Domain.Models.Media;

namespace Yugen.Domain.Data.Media;

public class EpisodeInfo
{
    public string? title { get; set; }
    public int number { get; set; }
    public string? thumbnail { get; set; }

    public bool isRecap { get; set; }
    public bool isFiller { get; set; }

    public float? score { get; set; }

    public string? jellyfinId { get; set; }
    public long? watchDate { get; set; }
    public float? watchPercentage { get; set; }

    public static EpisodeInfo Map(Model_MediaEpisode? episode, Model_DownloadedEpisode? download, Model_WatchedEpisode? watchTime)
    {
        return new EpisodeInfo()
        {
            title = episode?.EpisodeTitle ?? "Film",
            number = (episode?.EpisodeNumber ?? download?.EpisodeNumber!).Value,
            thumbnail = episode?.EpisodeIcon,

            isFiller = episode?.IsFiller ?? false,
            isRecap = episode?.IsRecap ?? false,

            score = episode?.Score,

            jellyfinId = download?.JellyfinId,
            watchDate = watchTime?.LastWatched != null ? new DateTimeOffset(watchTime.LastWatched!.Value).ToUnixTimeMilliseconds() : null,
            watchPercentage = watchTime?.WatchPercentage,
        };
    }
}

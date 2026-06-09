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

    public static EpisodeInfo Map((Model_MediaEpisode?, Model_DownloadedEpisode?, Model_WatchedEpisode?) res)
    {
        (Model_MediaEpisode? episode, Model_DownloadedEpisode? download, Model_WatchedEpisode? watchTime) = res;
        int number = (episode?.EpisodeNumber ?? download?.EpisodeNumber)!.Value;

        return new EpisodeInfo()
        {
            title = episode?.EpisodeTitle ?? $"Episode {number}",
            number = number,
            thumbnail = episode?.EpisodeIcon,

            isFiller = episode?.IsFiller ?? false,
            isRecap = episode?.IsRecap ?? false,

            score = episode?.Score,

            jellyfinId = download?.JellyfinId,
            watchDate = watchTime?.History?.UpdatedTime != null ? new DateTimeOffset(watchTime.History.UpdatedTime.Value).ToUnixTimeSeconds() : null,
            watchPercentage = watchTime?.WatchPercentage,
        };
    }


}

using Yugen.Domain.Models.Media;

namespace Yugen.Domain.Data.Media;

public class EpisodeInfo
{
    public string? title { get; set; }
    public int number { get; set; }

    public bool isRecap { get; set; }
    public bool isFiller { get; set; }

    public float? score { get; set; }

    public static EpisodeInfo Map(Model_MediaEpisode episode)
    {
        return new EpisodeInfo()
        {
            title = episode.EpisodeTitle,
            number = episode.EpisodeNumber,

            isFiller = episode.IsFiller,
            isRecap = episode.IsRecap,

            score = episode.Score
        };
    }
}

using Yugen.Domain.Models.History;
using Yugen.Domain.Models.Media;

namespace Yugen.Core.Data;

public class MediaCard
{
    public required int aniListId { get; set; }
    public string? Title { get; set; }
    public string? type { get; set; }

    public bool releasing { get; set; }
    public int? year { get; set; }
    public long? nextReleaseDate { get; set; }

    public string? colour { get; set; }
    public string? cardImg { get; set; }
    public string? banner { get; set; }

    public int? watchEpisode { get; set; }
    public long? watchLastTime { get; set; }
    public float? watchPercentage { get; set; }

    public static MediaCard Map(Model_Media dbData)
    {
        return new MediaCard()
        {
            aniListId = dbData.Id,
            Title = dbData.Title,

            year = dbData.Year,
            releasing = dbData.Status == "RELEASING",

            colour = dbData.Colour,
            cardImg = dbData.CardImageLarge,
            banner = dbData.BannerImage,

            type = dbData.MediaFormat,
        };
    }

    public MediaCard WithReleaseDate(long date)
    {
        nextReleaseDate = date;
        return this;
    }

    public MediaCard WithWatchInfo(Model_WatchedEpisode? ep)
    {
        if (ep == null)
            return this;

        watchPercentage = ep.WatchPercentage;
        watchEpisode = ep.EpisodeNumber;
        watchLastTime = ep.LastWatched?.Ticks;

        return this;
    }
}

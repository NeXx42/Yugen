using Yugen.Domain.Data;
using Yugen.Domain.Models.History;
using Yugen.Domain.Models.Media;

namespace Yugen.Core.Data;

public class MediaCard
{
    public required int aniListId { get; set; }
    public string? Title { get; set; }
    public string? type { get; set; }

    public string? status { get; set; }
    public long? nextReleaseDate { get; set; }

    public int? year { get; set; }
    public string? season { get; set; }

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
            Title = dbData.TitleEnglish ?? dbData.TitleNative,

            year = dbData.Year,
            season = dbData.Season,

            status = dbData.Status,
            nextReleaseDate = dbData.NextEpisodeReleaseDate,

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

    public MediaCard WithWatchInfo(Model_WatchHistory his, Model_WatchedEpisode? ep)
    {
        if (ep == null)
            return this;

        watchPercentage = ep.WatchPercentage;
        watchEpisode = ep.EpisodeNumber;
        watchLastTime = his?.UpdatedTime != null ? new DateTimeOffset(his.UpdatedTime.Value).ToUnixTimeSeconds() : null;

        return this;
    }

    public bool IsInFilter(MediaSearchQuery? query)
    {
        if (query == null)
            return true;

        if (!string.IsNullOrEmpty(query.season) && !(season?.Equals(query.season) ?? false))
            return false;

        if (!string.IsNullOrEmpty(query.format) && !(type?.Equals(query.format) ?? false))
            return false;

        if (!string.IsNullOrEmpty(query.status) && !(status?.Equals(query.status) ?? false))
            return false;

        if (query.year.HasValue && year != query.year)
            return false;

        return true;
    }
}

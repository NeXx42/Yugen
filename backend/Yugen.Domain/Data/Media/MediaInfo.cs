using Yugen.Core.Data;
using Yugen.Domain.Models;
using Yugen.Domain.Models.Bookmarks;
using Yugen.Domain.Models.Linking;
using Yugen.Domain.Models.Media;

namespace Yugen.Domain.Data.Media;

public class MediaInfo
{
    public int id { get; set; }

    public string? title { get; set; }
    public string? description { get; set; }
    public string? type { get; set; }

    public string? thumbnailImage { get; set; }
    public string? bannerImage { get; set; }
    public string? cardImage { get; set; }
    public string? colour { get; set; }

    public string? status { get; set; }
    public long? startDate { get; set; }
    public long? endDate { get; set; }
    public int? episodeCount { get; set; }
    public int? duration { get; set; }
    public string? season { get; set; }
    public long? upcomingEpisode { get; set; }

    public int? bookmark { get; set; }

    public MediaTag[]? tags { get; set; }
    public MediaCard[]? recommended { get; set; }
    public Connection[]? connectedMedia { get; set; }

    public static MediaInfo Map(Model_Media media)
    {
        return new MediaInfo()
        {
            id = media.Id,
            title = media.Title,
            description = media.Description,
            type = media.MediaFormat,

            status = media.Status,
            startDate = media.StartDate,
            endDate = media.EndDate,
            episodeCount = media.EpisodeCount,
            duration = media.Duration,
            season = media.Season,
            upcomingEpisode = media.NextEpisodeReleaseDate,

            thumbnailImage = media.thumbnailIcon,
            bannerImage = media.BannerImage,
            cardImage = media.CardImageLarge,
            colour = media.Colour,
        };
    }

    public MediaInfo RegisterConnectedMedia((Model_Link link, MediaCard media)[]? media)
    {
        if (media == null)
            return this;

        connectedMedia = media?.Select(m => new Connection()
        {
            season = m.link.tvdb_season,
            type = m.link.type,

            card = m.media,
        }).ToArray();
        return this;
    }

    public MediaInfo RegisterBookmark(Model_UserBookmark? bookmark)
    {
        this.bookmark = bookmark?.BookmarkId;
        return this;
    }

    public MediaInfo RegisterTags(Model_Tag?[]? tags)
    {
        this.tags = tags?.Where(t => t != null).Select(t => new MediaTag()
        {
            id = t!.Id,
            title = t.Name ?? ""
        }).ToArray();

        return this;
    }

    public MediaInfo RegisterRelated(MediaCard[] cards)
    {
        this.recommended = cards;
        return this;
    }

    public class Connection
    {
        public int? season { get; set; }
        public string? type { get; set; }
        public required MediaCard card { get; set; }
    }
}

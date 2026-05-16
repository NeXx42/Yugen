using Yugen.Domain.Models.Media;

namespace Yugen.Domain.Data.Media;

public class MediaInfo
{
    public int id { get; set; }

    public string? title { get; set; }

    public string? bannerImage { get; set; }
    public string? cardImage { get; set; }
    public string? colour { get; set; }

    public EpisodeInfo[]? episodes { get; set; }

    public static MediaInfo Map(Model_Media media)
    {
        return new MediaInfo()
        {
            id = media.Id,

            title = media.Title,
            bannerImage = media.BannerImage,
            cardImage = media.CardImageLarge,
            colour = media.Colour,

            episodes = media.Episodes.Select(EpisodeInfo.Map).ToArray()
        };
    }
}

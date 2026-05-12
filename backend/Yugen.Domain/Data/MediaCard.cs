using Yugen.Domain.Models.Media;

namespace Yugen.Core.Data;

public class MediaCard
{
    public required int aniListId { get; set; }
    public string? Title { get; set; }

    public long? nextReleaseDate { get; set; }

    public string? colour { get; set; }
    public string? cardImg { get; set; }

    public static MediaCard Map(Model_Media dbData)
    {
        return new MediaCard()
        {
            aniListId = dbData.Id,
            Title = dbData.Title,

            colour = dbData.Colour,
            cardImg = dbData.CardImageLarge
        };
    }

    public MediaCard WithReleaseDate(long date)
    {
        nextReleaseDate = date;
        return this;
    }
}

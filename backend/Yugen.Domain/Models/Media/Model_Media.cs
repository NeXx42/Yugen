using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yugen.Domain.Models.Linking;

namespace Yugen.Domain.Models.Media;

public class Model_Media
{
    // is the anilist id
    [Key]
    [Required]
    public required int Id { get; set; }

    public string? TitleNative { get; set; }
    public string? TitleEnglish { get; set; }
    public string? MediaFormat { get; set; }

    public string? BannerImage { get; set; }
    public string? CardImageSmall { get; set; }
    public string? CardImageLarge { get; set; }
    public string? Colour { get; set; }
    public string? thumbnailIcon { get; set; }

    public int? Duration { get; set; }
    public int? EpisodeCount { get; set; }
    public long? StartDate { get; set; }
    public long? EndDate { get; set; }
    public string? Season { get; set; }
    public int? Year { get; set; }

    public string? Description { get; set; }
    public string? Status { get; set; }
    public string? SiteUrl { get; set; }

    public int? AverageScore { get; set; }
    public int? MeanScore { get; set; }

    public long? LastUpdated { get; set; }
    public long? NextEpisodeReleaseDate { get; set; }

    public Collection<Model_MediaTag> Tags { get; set; } = new Collection<Model_MediaTag>();
    public Collection<Model_MediaGenre> Genres { get; set; } = new Collection<Model_MediaGenre>();
    public Collection<Model_MediaEpisode> Episodes { get; set; } = new Collection<Model_MediaEpisode>();
    public Collection<Model_MediaRelation> RelatedMedia { get; set; } = new Collection<Model_MediaRelation>();

    public void Update(Model_Media fresh)
    {
        this.TitleEnglish = fresh.TitleNative;
        this.TitleNative = fresh.TitleNative;
        this.MediaFormat = fresh.MediaFormat;

        this.BannerImage = fresh.BannerImage ?? BannerImage;
        this.CardImageSmall = fresh.CardImageSmall ?? CardImageSmall;
        this.CardImageLarge = fresh.CardImageLarge ?? CardImageLarge;
        this.Colour = fresh.Colour ?? Colour;
        this.thumbnailIcon = fresh.thumbnailIcon ?? thumbnailIcon;

        this.Duration = fresh.Duration ?? Duration;
        this.EpisodeCount = fresh.EpisodeCount ?? EpisodeCount;
        this.StartDate = fresh.StartDate ?? StartDate;
        this.EndDate = fresh.EndDate ?? EndDate;
        this.Season = fresh.Season ?? Season;
        this.Year = fresh.Year ?? Year;

        this.Description = fresh.Description ?? Description;
        this.Status = fresh.Status ?? Status;
        this.SiteUrl = fresh.SiteUrl ?? SiteUrl;

        this.AverageScore = fresh.AverageScore ?? AverageScore;
        this.MeanScore = fresh.MeanScore ?? MeanScore;

        LastUpdated = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        NextEpisodeReleaseDate = fresh.NextEpisodeReleaseDate;
    }
}

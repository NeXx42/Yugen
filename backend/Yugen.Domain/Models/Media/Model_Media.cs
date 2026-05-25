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

    public string? Title { get; set; }
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

    public bool? Hydrated { get; set; }
    public long? NextEpisodeReleaseDate { get; set; }

    public Collection<Model_MediaTag> Tags { get; set; } = new Collection<Model_MediaTag>();
    public Collection<Model_MediaEpisode> Episodes { get; set; } = new Collection<Model_MediaEpisode>();
    public Collection<Model_MediaRelation> RelatedMedia { get; set; } = new Collection<Model_MediaRelation>();
}

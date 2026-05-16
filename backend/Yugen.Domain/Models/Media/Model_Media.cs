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

    public string? BannerImage { get; set; }
    public string? CardImageSmall { get; set; }
    public string? CardImageLarge { get; set; }
    public string? Colour { get; set; }
    public int EpisodeCount { get; set; }

    public bool? Hydrated { get; set; }

    public Collection<Model_MediaEpisode> Episodes { get; set; } = new Collection<Model_MediaEpisode>();
}

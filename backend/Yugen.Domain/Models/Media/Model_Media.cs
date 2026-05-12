using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace Yugen.Domain.Models.Media;

public class Model_Media
{
    // is the anilist id
    [Key]
    [Required]
    public required int Id { get; set; }
    public int? MalId { get; set; }

    public string? Title { get; set; }

    public string? BannerImage { get; set; }
    public string? CardImageSmall { get; set; }
    public string? CardImageLarge { get; set; }
    public string? Colour { get; set; }
    public int EpisodeCount { get; set; }

    public bool? Hydrated { get; set; }

    public int? SuccessorId { get; set; } = null;
    public Model_Media? Successor { get; set; } = null;
    public Model_Media? Predecessor { get; set; } = null;

    public Collection<Model_MediaEpisode> Episodes { get; set; } = new Collection<Model_MediaEpisode>();
}

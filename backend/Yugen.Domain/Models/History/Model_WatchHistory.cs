using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace Yugen.Domain.Models.History;

public class Model_WatchHistory
{
    [Required]
    [Key]
    public int Id { get; set; }

    [Required]
    public required Guid UserId { get; set; }

    [Required]
    public required int MediaId { get; set; }

    public DateTime? UpdatedTime { get; set; }
    public int? LastWatchedEpisodeNumber { get; set; }

    public ICollection<Model_WatchedEpisode> WatchedEpisodes = new List<Model_WatchedEpisode>();
}

using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace Yugen.Domain.Models.History;

public class Model_WatchHistory
{
    [Key]
    public int MediaId { get; set; }

    public int? WatchedEpisode { get; set; }
    public DateTime? UpdatedTime { get; set; }

    public ICollection<Model_WatchedEpisode> WatchedEpisodes = new List<Model_WatchedEpisode>();
}

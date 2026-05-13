using System.ComponentModel.DataAnnotations;
using Yugen.Domain.Data.Downloads;

namespace Yugen.Domain.Models.Library;

public class Model_DownloadedMedia
{
    [Key]
    [Required]
    public required int MediaId { get; set; }

    public int? AniDbId { get; set; }

    public bool IsMonitored { get; set; }
    public DateTime LastChecked { get; set; }

    public ICollection<Model_DownloadedEpisode> downloadedEpisodes { get; set; } = new List<Model_DownloadedEpisode>();
}

using System.ComponentModel.DataAnnotations;
using Yugen.Domain.Data.Downloads;

namespace Yugen.Domain.Models.Library;

public class Model_DownloadedMedia
{
    [Key]
    [Required]
    public required int MediaId { get; set; }

    [Required]
    public required int ProviderId { get; set; }

    [Required]
    public required int SeasonId { get; set; }

    public string? ExternalRoot { get; set; }
    public int? ExternalQuality { get; set; }

    public bool IsMonitored { get; set; }
    public DateTime LastChecked { get; set; }

    public ICollection<Model_DownloadedEpisode> downloadedEpisodes { get; set; } = new List<Model_DownloadedEpisode>();
}

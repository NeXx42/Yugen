using System.ComponentModel.DataAnnotations;

namespace Yugen.Domain.Models.Library;

public class Model_DownloadedEpisode
{
    [Required]
    public required int MediaId { get; set; }
    public Model_DownloadedMedia? DownloadedMedia { get; set; }

    [Required]
    public required int EpisodeNumber { get; set; }

    public int? sonarrEpisodeId { get; set; }
    public string? filePath { get; set; }
    public string? JellyfinId { get; set; }
}

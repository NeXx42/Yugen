using System.ComponentModel.DataAnnotations;

namespace Yugen.Domain.Models.Library;

public class Model_DownloadedMedia
{
    // is the anilist id
    [Key]
    [Required]
    public required int Id { get; set; }
}

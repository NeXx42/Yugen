using System.ComponentModel.DataAnnotations;

namespace Yugen.Domain.Models.Media;

public class Model_MediaGenre
{
    [Key]
    [Required]
    public required int MediaId { get; set; }
    public Model_Media? Media { get; set; }

    [Required]
    public required string Genre { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace Yugen.Domain.Models.Media;

public class Model_MediaTag
{
    [Key]
    [Required]
    public required int MediaId { get; set; }
    public Model_Media? Media { get; set; }

    [Key]
    [Required]
    public required int TagId { get; set; }
    public Model_Tag? Tag { get; set; }
}

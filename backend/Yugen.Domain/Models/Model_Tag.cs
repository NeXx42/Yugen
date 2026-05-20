using System.ComponentModel.DataAnnotations;

namespace Yugen.Domain.Models;

public class Model_Tag
{
    [Key]
    [Required]
    public required int Id { get; set; }

    public bool IsAdult { get; set; }
    public bool IsMediaSpoiler { get; set; }
    public bool IsGeneralSpoiler { get; set; }

    public string? Name { get; set; }
    public string? Category { get; set; }
    public string? Description { get; set; }
}

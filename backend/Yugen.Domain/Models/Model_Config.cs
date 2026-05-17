using System.ComponentModel.DataAnnotations;

namespace Yugen.Domain.Models;

public class Model_Config
{
    [Key]
    [Required]
    public required string Key { get; set; }
    public required string Value { get; set; }
}

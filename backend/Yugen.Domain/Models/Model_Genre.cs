using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yugen.Domain.Models;

public class Model_Genre
{
    [Key]
    [Required]
    public required string Genre { get; set; }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yugen.Domain.Models.Bookmarks;

public class Model_Bookmark
{
    [Key]
    [Required]
    public int Id { get; set; }

    [Required]
    public required string Title { get; set; }
}

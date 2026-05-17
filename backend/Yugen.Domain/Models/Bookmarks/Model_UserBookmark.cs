using System.ComponentModel.DataAnnotations;

namespace Yugen.Domain.Models.Bookmarks;

public class Model_UserBookmark
{
    [Required]
    public required int MediaId { get; set; }

    [Required]
    public required Guid UserId { get; set; }

    [Required]
    public required int BookmarkId { get; set; }
    public Model_Bookmark? Bookmark { get; set; }

    [Required]
    public required DateTime DateAdded { get; set; }
}

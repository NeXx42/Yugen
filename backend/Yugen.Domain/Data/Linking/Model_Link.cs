using System.ComponentModel.DataAnnotations;

namespace Yugen.Domain.Data.Linking;

public class Model_Link
{
    [Required]
    [Key]
    public required int anidbid { set; get; } ///...............!!!

    public int? tvdbid { get; set; }
    public int? defaulttvdbseason { get; set; }
    public int? tmdbtv { get; set; }
    public int? tmdbseason { get; set; }
}

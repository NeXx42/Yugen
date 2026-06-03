using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yugen.Domain.Enums;

namespace Yugen.Domain.Models;

public class Model_Notification
{
    [Key]
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int? MediaId { get; set; }
    public int? MediaEpisode { get; set; }

    [Required]
    public required Guid UserId { get; set; }

    [Required]
    public required string EventName { get; set; }

    [Required]
    public required DateTime Date { get; set; }

    public string? Message { get; set; }
    public string? Source { get; set; }
    public bool HasInteracted { get; set; }
}

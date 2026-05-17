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

    [Required]
    public required int MediaId { get; set; }

    [Required]
    public required Guid UserId { get; set; }

    [Required]
    public required SonarrWebhookEventType EventType { get; set; }

    [Required]
    public required DateTime Date { get; set; }

    public string? Message { get; set; }
    public bool HasInteracted { get; set; }
}

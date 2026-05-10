using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yugen.Domain.Enums;

namespace Yugen.Domain.Models;

public class MediaExternalProviderModel
{
    [Required]
    public Guid MediaId { get; set; }
    public MediaModel? Media { get; set; } = null;

    [Required]
    public ProviderType ProviderType { get; set; }

    [Required]
    public required string ExternalIdentity { get; set; }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yugen.Domain.Models;

public class MediaModel
{
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public string? Title { get; set; }

    public ICollection<MediaExternalProviderModel> externalProviders { get; set; } = new List<MediaExternalProviderModel>();
}

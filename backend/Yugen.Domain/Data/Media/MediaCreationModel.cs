using Yugen.Domain.Models.Media;

namespace Yugen.Domain.Data.Media;

public class MediaCreationModel : Model_Media
{
    public required string providerId { get; set; }
}

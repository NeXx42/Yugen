using Yugen.Domain.Models.Media;

namespace Yugen.Domain.Data.Media;

public class MediaEpisodeCreationModel : Model_MediaEpisode
{
    public required string providerId { get; set; }
}

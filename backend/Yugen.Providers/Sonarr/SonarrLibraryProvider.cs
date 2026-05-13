using Yugen.Domain.Data.Media;
using Yugen.Domain.Models.Media;

namespace Yugen.Providers.Sonarr;

public class SonarrLibraryProvider : ILibraryProvider
{
    public Task<bool?> GetDownloadedMedia(Model_Media jellyfinUserId)
    {
        throw new NotImplementedException();
    }
}

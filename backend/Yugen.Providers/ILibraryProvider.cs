using Yugen.Domain.Data.Media;
using Yugen.Domain.Data.Users;
using Yugen.Domain.Models.Media;

namespace Yugen.Providers;

public interface ILibraryProvider
{
    public Task<bool?> GetDownloadedMedia(Model_Media jellyfinUserId);
}

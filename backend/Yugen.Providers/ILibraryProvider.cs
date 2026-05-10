using Yugen.Domain.Data.Media;
using Yugen.Domain.Data.Users;

namespace Yugen.Providers;

public interface ILibraryProvider
{
    public Task<ExternalMedia[]> GetExternalMedia(string jellyfinUserId);
}

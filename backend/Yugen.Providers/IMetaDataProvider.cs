using Yugen.Core.Data;

namespace Yugen.Providers;

public interface IMetaDataProvider
{
    public Task<MediaCard[]> SearchMedia(string textFilter);
}

using Yugen.Core.Data;
using Yugen.Providers;
using Yugen.Providers.AniList;

namespace Yugen.Core.Services;

public class CatalogService
{
    private readonly IMetaDataProvider _currentProvider;

    public CatalogService()
    {
        _currentProvider = new AniListProvider();
    }

    public async Task<MediaCard[]> Search(string textFilter)
    {
        return await _currentProvider.SearchMedia(textFilter);
    }
}

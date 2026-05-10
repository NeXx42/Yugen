using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Yugen.Core.Data;
using Yugen.Data;
using Yugen.Domain.Data.Media;
using Yugen.Domain.Models;
using Yugen.Providers;
using Yugen.Providers.AniList;

namespace Yugen.Core.Services;

public class CatalogService
{
    private readonly YugenContext _db;
    private readonly IMetaDataProvider _currentProvider;

    public CatalogService(YugenContext db)
    {
        _db = db;
        _currentProvider = new AniListProvider();
    }

    public async Task<MediaCard[]> Search(string textFilter)
    {
        return await _currentProvider.SearchMedia(textFilter);
    }

    public async Task<MediaInfo> GetMediaInfo(Guid internalId)
    {
        MediaModel? downloadedMedia = await _db.media.FirstOrDefaultAsync(x => x.Id == internalId);

        return new MediaInfo()
        {
            title = internalId.ToString(),
            isDownloaded = downloadedMedia != null,
        };
    }
}

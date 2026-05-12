using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Yugen.Core.Data;
using Yugen.Data;
using Yugen.Domain.Data.Media;
using Yugen.Domain.Enums;
using Yugen.Domain.Models;
using Yugen.Domain.Models.Media;
using Yugen.Providers;
using Yugen.Providers.AniList;

namespace Yugen.Core.Services;

public class CatalogService
{
    private readonly YugenContext _db;

    private readonly IMetaDataProvider _currentProvider;

    private readonly CacheService _cache;
    private readonly HydrationService _hydrationService;

    public CatalogService(YugenContext db, HydrationService hydrationService, CacheService cache)
    {
        _db = db;

        _currentProvider = new AniListProvider();

        _cache = cache;
        _hydrationService = hydrationService;
    }

    public async Task<MediaCard[]> Search(string textFilter)
    {
        return await _currentProvider.SearchMedia(textFilter);
    }

    public async Task<MediaInfo> GetMediaInfo(int aniListId)
    {
        string CACHE_KEY = $"{nameof(GetMediaInfo)}_{aniListId}";

        if (_cache.TryGetValue(CACHE_KEY, out MediaInfo? info))
            return info!;

        Model_Media? dbEntry = await _db.media.Include(m => m.Episodes).FirstOrDefaultAsync(m => m.Id == aniListId);

        if (dbEntry == null)
        {
            dbEntry = await _hydrationService.SaveMedia(aniListId);

            if (dbEntry == null)
                throw new FileNotFoundException();
        }

        await _hydrationService.HydrateMedia(dbEntry);
        info = MediaInfo.Map(dbEntry);

        _cache.SetIfNotExists(CACHE_KEY, info);

        return info;
    }

    public async Task UpdateMetadata(Guid internalId)
    {


    }

    public async Task<MediaCard[]> Upcoming()
    {
        const string CACHE_KEY = "CATALOG_UPCOMING";

        if (!_cache.TryGetValue(CACHE_KEY, out Dictionary<int, long>? upcoming) || upcoming == null)
        {
            upcoming = await _currentProvider.UpcomingMedia();

            _cache.Remove(CACHE_KEY);
            _cache.SetIfNotExists(CACHE_KEY, upcoming);
        }

        List<int> newIds = upcoming.Keys.ToList();
        Model_Media[] cachedMedia = await _db.media.Where(m => newIds.Contains(m.Id)).ToArrayAsync();

        List<int> uncached = new List<int>();
        List<MediaCard> results = new List<MediaCard>();

        foreach (Model_Media media in cachedMedia)
        {
            newIds.Remove(media.Id);
            results.Add(MediaCard.Map(media));
        }

        Model_Media[] newMedia = await _hydrationService.SaveMedia(newIds);
        results.AddRange(newMedia.Select(MediaCard.Map));

        return results.Select(x => x.WithReleaseDate(upcoming[x.aniListId])).ToArray();
    }
}

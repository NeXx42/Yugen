using System.Xml;
using EFCore.BulkExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Yugen.Core.Data;
using Yugen.Data;
using Yugen.Domain.Data.Linking;
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
        const string CACHE_KEY = "SearchResults";
        const string SEARCH_CACHE = $"SearchFilter";

        MediaCard[]? cards;

        if (_cache.TryGetValue(SEARCH_CACHE, out string? res))
        {
            if (res == textFilter)
            {
                if (_cache.TryGetValue(CACHE_KEY, out cards))
                    return cards!;
            }
        }

        if (string.IsNullOrEmpty(textFilter))
            throw new ArgumentException();

        int[] media = await _currentProvider.SearchMedia(textFilter);
        cards = await GetOrCreateMediaCardsFromIds(media.ToList());

        _cache.Set(CACHE_KEY, cards);
        _cache.Set(SEARCH_CACHE, textFilter);

        return cards;
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

        MediaCard[] media = await GetOrCreateMediaCardsFromIds(upcoming.Keys.ToList());
        return media.Select(x => x.WithReleaseDate(upcoming[x.aniListId])).ToArray();
    }

    private async Task<MediaCard[]> GetOrCreateMediaCardsFromIds(List<int> ids)
    {
        Model_Media[] cachedMedia = await _db.media.Where(m => ids.Contains(m.Id)).ToArrayAsync();

        List<int> uncached = new List<int>();
        List<MediaCard> results = new List<MediaCard>();

        foreach (Model_Media media in cachedMedia)
        {
            ids.Remove(media.Id);
            results.Add(MediaCard.Map(media));
        }

        Model_Media[] newMedia = await _hydrationService.SaveMedia(ids);
        return [.. results, .. newMedia.Select(MediaCard.Map)];
    }

    public async Task RedownloadLinks()
    {
        const string url = "https://raw.githubusercontent.com/Anime-Lists/anime-lists/refs/heads/master/anime-list-full.xml";
        HttpClient client = new HttpClient();
        HttpResponseMessage res = await client.GetAsync(url);

        List<Model_Link> links = new List<Model_Link>();

        using (XmlReader reader = XmlReader.Create(await res.Content.ReadAsStreamAsync()))
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "anime")
                {
                    var anilist = reader.GetAttribute("anidbid");

                    if (int.TryParse(anilist, out var aniId))
                    {
                        links.Add(new Model_Link
                        {
                            anidbid = aniId,

                            tvdbid = TryGetIntValue("tvdbid"),
                            defaulttvdbseason = TryGetIntValue("defaulttvdbseason"),
                            tmdbtv = TryGetIntValue("tmdbtv"),
                            tmdbseason = TryGetIntValue("tmdbseason"),
                        });
                    }

                    int? TryGetIntValue(string attribute)
                    {
                        string? str = reader.GetAttribute(attribute);

                        if (int.TryParse(str, out int res))
                            return res;

                        return null;
                    }
                }
            }
        }

        await _db.BulkInsertOrUpdateAsync(links);

    }
}

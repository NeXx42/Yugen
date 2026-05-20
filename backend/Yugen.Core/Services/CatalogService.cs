using System.Net.Http.Json;
using System.Xml;
using Azure;
using EFCore.BulkExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Yugen.Core.Data;
using Yugen.Data;
using Yugen.Domain.Data;
using Yugen.Domain.Data.Media;
using Yugen.Domain.Data.Users;
using Yugen.Domain.Enums;
using Yugen.Domain.Models;
using Yugen.Domain.Models.Bookmarks;
using Yugen.Domain.Models.Linking;
using Yugen.Domain.Models.Media;
using Yugen.Providers;
using Yugen.Providers.AniList;

namespace Yugen.Core.Services;

public class CatalogService
{
    private readonly YugenContext _db;

    private readonly IMetaDataProvider _currentProvider;

    private readonly CacheService _cache;
    private readonly SettingsService _settings;
    private readonly HydrationService _hydrationService;

    public string GetCardCacheId(int id) => $"CardCache_{id}";
    public string GetInfoCacheId(int id) => $"Info_{id}";

    public CatalogService(YugenContext db, HydrationService hydrationService, CacheService cache, SettingsService settings)
    {
        _db = db;

        _currentProvider = new AniListProvider();

        _cache = cache;
        _settings = settings;
        _hydrationService = hydrationService;
    }

    public async Task<PageResponse<MediaCard>> Search(MediaSearchQuery query)
    {
        const string CACHE_KEY = "SearchResults";
        const string SEARCH_CACHE = $"SearchFilter";

        string searchCacheValue = query.GetCacheKey();

        PageResponse<MediaCard>? pageResponse;

        if (_cache.TryGetValue(SEARCH_CACHE, out string? res))
        {
            if (res == searchCacheValue)
            {
                if (_cache.TryGetValue(CACHE_KEY, out pageResponse))
                    return pageResponse!;
            }
        }

        (int total, int[] media) = await _currentProvider.SearchMedia(query, _settings.getCache.Get(ConfigKeys.AdultContent, false));
        pageResponse = new PageResponse<MediaCard>(await GetOrCreateMediaCardsFromIds(media.ToList()), query.page ?? 1, query.pageSize ?? 10, total);

        _cache.Set(CACHE_KEY, pageResponse);
        _cache.Set(SEARCH_CACHE, searchCacheValue);

        return pageResponse;
    }

    public async Task<MediaInfo> GetMediaInfoForUser(UserSession usr, int aniListId)
    {
        MediaInfo info = await GetMediaInfo(aniListId);
        Model_UserBookmark? bookmark = await _db.userBookmarks.FirstOrDefaultAsync(b => b.UserId == usr.User.Id && b.MediaId == aniListId);

        return info.RegisterBookmark(bookmark);
    }

    public async Task<MediaInfo> GetMediaInfo(int aniListId)
    {
        string cacheId = GetInfoCacheId(aniListId);

        if (_cache.TryGetValue(cacheId, out MediaInfo? info) && info != null)
            return info;

        Model_Media? dbEntry = await _db.media.FirstOrDefaultAsync(m => m.Id == aniListId);

        if (dbEntry == null)
        {
            dbEntry = await _hydrationService.SaveMedia(aniListId);

            if (dbEntry == null)
                throw new FileNotFoundException();
        }

        (int? season, string? type, MediaCard model)[]? connectedMedia = null;
        Model_Link? link = await _db.links.FirstOrDefaultAsync(l => l.anilist_id == aniListId);

        if (link != null && link.tvdb_id.HasValue)
        {
            var connections = await _db.links.Where(l => l.tvdb_id == link.tvdb_id).ToArrayAsync();
            MediaCard[] connectionCards = await GetOrCreateMediaCardsFromIds(connections.Where(c => c.anilist_id.HasValue).Select(c => c.anilist_id!.Value).ToList());

            connectedMedia = connections.Select(l => (l.tvdb_season, l.type, connectionCards.Single(c => c.aniListId == l.anilist_id))).ToArray();
        }

        info = MediaInfo.Map(dbEntry).RegisterConnectedMedia(connectedMedia);

        _cache.Set(cacheId, info);
        return info;
    }

    public async Task<MediaCard[]> Upcoming(int take)
    {
        const string CACHE_KEY = "CATALOG_UPCOMING";

        if (!_cache.TryGetValue(CACHE_KEY, out Dictionary<int, long>? upcoming) || upcoming == null)
        {
            upcoming = await _currentProvider.UpcomingMedia();

            _cache.Remove(CACHE_KEY);
            _cache.SetIfNotExists(CACHE_KEY, upcoming, new TimeSpan(0, 30, 0));
        }

        MediaCard[] media = await GetOrCreateMediaCardsFromIds(upcoming.Keys.ToList());
        return media.Select(x => x.WithReleaseDate(upcoming[x.aniListId])).OrderBy(x => x.nextReleaseDate).Take(take).ToArray();
    }

    public async Task<MediaCard[]> GetOrCreateMediaCardsFromIds(List<int> ids)
    {
        MediaCard? card;
        List<MediaCard> results = new List<MediaCard>();

        for (int i = ids.Count - 1; i >= 0; i--)
        {
            if (_cache.TryGetValue(GetCardCacheId(ids[i]), out card) && card != null)
            {
                results.Add(card);
                ids.RemoveAt(i);
            }
        }

        if (ids.Count == 0)
            return results.ToArray();

        Model_Media[] existingDbEntries = await _db.media.Where(m => ids.Contains(m.Id)).ToArrayAsync();

        foreach (Model_Media media in existingDbEntries)
        {
            card = MediaCard.Map(media);
            _cache.Set(GetCardCacheId(card.aniListId), card);

            ids.Remove(media.Id);
            results.Add(card);
        }

        IEnumerable<MediaCard> newCards = (await _hydrationService.SaveMedia(ids)).Select(MediaCard.Map);

        foreach (MediaCard newCard in newCards)
            _cache.Set(GetCardCacheId(newCard.aniListId), newCard);

        return [.. results, .. newCards];
    }

    public async Task<long?> GetTimeOfNextEpisode(int id)
    {
        string cacheKey = $"{nameof(GetTimeOfNextEpisode)}_{id}";

        if (_cache.TryGetValue(cacheKey, out long? unixTime))
            return unixTime;

        unixTime = await _currentProvider.GetTimeOfNextEpisode(id);
        TimeSpan cacheDuration = unixTime.HasValue ? DateTimeOffset.FromUnixTimeSeconds(unixTime.Value) - DateTime.UtcNow : new TimeSpan(12, 0, 0);

        _cache.Set(cacheKey, unixTime, cacheDuration);
        return unixTime;
    }

    public async Task<MediaInfo[]> GetTrending(int limit)
    {
        string cacheKey = $"{nameof(GetTrending)}_{limit}";

        if (_cache.TryGetValue(cacheKey, out MediaInfo[]? cards))
            return cards ?? [];

        List<int>? ids = await _currentProvider.GetTrending(limit);

        if (ids == null)
            return [];

        cards = new MediaInfo[ids.Count];

        for (int i = 0; i < ids.Count; i++)
            cards[i] = await GetMediaInfo(ids[i]);

        _cache.Set(cacheKey, cards);
        return cards;
    }

    public async Task ClearDatabaseCache()
    {
        _db.RemoveRange(await _db.mediaEpisodes.ToListAsync());
        _db.RemoveRange(await _db.media.ToListAsync());
        await _db.SaveChangesAsync();

        await ClearCache();
    }

    public async Task ClearCache() => _cache.Clear();

    public async Task RedownloadLinks()
    {
        const string url = "https://raw.githubusercontent.com/Fribb/anime-lists/refs/heads/master/anime-list-full.json";

        using (HttpClient client = new HttpClient())
        {
            HttpResponseMessage res = await client.GetAsync(url);
            Link[]? links = await res.Content.ReadFromJsonAsync<Link[]>();

            if (links == null)
                return;

            List<Model_Link> newLinks = new List<Model_Link>();

            foreach (Link l in links)
            {
                if (l.anilist_id == null)
                    continue;

                newLinks.Add(new Model_Link()
                {
                    anilist_id = l.anilist_id,
                    anidb_id = l.anidb_id,
                    animecountdown_id = l.animecountdown_id,
                    animenewsnetwork_id = l.animenewsnetwork_id,
                    anime_planet_id = l.anime_planet_id,
                    anisearch_id = l.anisearch_id,
                    imdb_id = l.imdb_id,
                    kitsu_id = l.kitsu_id,
                    livechart_id = l.livechart_id,
                    mal_id = l.mal_id,
                    simkl_id = l.simkl_id,
                    themoviedb_id = l.themoviedb_id,
                    tmdb_season = l.season?.tmdb,
                    tvdb_id = l.tvdb_id,
                    tvdb_season = l.season?.tvdb,
                    type = l.type
                });
            }

            try
            {
                await _db.BulkInsertOrUpdateAsync(newLinks);
                await _db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }

    public class Link
    {
        public int? anilist_id { get; set; }

        public string? type { get; set; }
        public int? anidb_id { get; set; }
        public int? animecountdown_id { get; set; }
        public int? animenewsnetwork_id { get; set; }
        public string? anime_planet_id { get; set; }
        public int? anisearch_id { get; set; }
        public string? imdb_id { get; set; }
        public int? kitsu_id { get; set; }
        public int? livechart_id { get; set; }
        public int? mal_id { get; set; }
        public int? simkl_id { get; set; }
        public int? themoviedb_id { get; set; }
        public int? tvdb_id { get; set; }

        public Season? season { get; set; }

        public class Season
        {
            public int? tvdb { get; set; }
            public int? tmdb { get; set; }
        }
    }

    public async Task<SearchCriteria> GetSearchCriteria()
    {
        if (!_settings.getCache.Get(ConfigKeys.HasSearchCriteriaCached, false))
        {
            await RedownloadCriteria();
            await _settings.SetConfigValue(ConfigKeys.HasSearchCriteriaCached, true);
        }

        string[] genres = await _db.genres.Select(g => g.Genre).ToArrayAsync();
        SearchCriteria.LookupPair[] tags = (await _db.tags.ToArrayAsync()).Select(t => new SearchCriteria.LookupPair()
        {
            id = t.Id,
            name = t.Name ?? "ERROR",
        }).ToArray();

        return new SearchCriteria()
        {
            genres = genres,
            tags = tags,
        };
    }

    public async Task RedownloadCriteria()
    {
        (List<Model_Tag> tags, List<Model_Genre> genres) = await _currentProvider.GetSearchCriteria();

        _db.RemoveRange(_db.tags);
        _db.RemoveRange(_db.genres);
        await _db.SaveChangesAsync();

        await _db.BulkInsertAsync(tags);
        await _db.BulkInsertAsync(genres);
    }
}

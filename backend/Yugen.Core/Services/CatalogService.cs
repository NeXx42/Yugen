using System.Net.Http.Json;
using System.Xml;
using EFCore.BulkExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Yugen.Core.Data;
using Yugen.Data;
using Yugen.Domain.Data.Media;
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

        (int? season, string type, Model_Media model)[]? connectedMedia = null;
        Model_Link? link = await _db.links.FirstOrDefaultAsync(l => l.anilist_id == aniListId);

        if (link != null && link.tvdb_id.HasValue)
        {
            connectedMedia = (await
            (
                from l in _db.links
                join m in _db.media
                    on l.anilist_id equals m.Id into mediaJoin
                from m in mediaJoin.DefaultIfEmpty()
                where l.tvdb_id == link.tvdb_id
                select new
                {
                    l.tvdb_season,
                    l.type,
                    model = m
                }
            ).ToArrayAsync()).Select(r => (r.tvdb_season, r.type, r.model)).ToArray();
        }

        await _hydrationService.HydrateMedia(dbEntry, link);
        info = MediaInfo.Map(dbEntry).RegisterConnectedMedia(connectedMedia);

        _cache.SetIfNotExists(CACHE_KEY, info);

        return info;
    }

    public async Task UpdateMetadata(Guid internalId)
    {


    }

    public async Task<MediaCard[]> Upcoming(int take)
    {
        const string CACHE_KEY = "CATALOG_UPCOMING";

        if (!_cache.TryGetValue(CACHE_KEY, out Dictionary<int, long>? upcoming) || upcoming == null)
        {
            upcoming = await _currentProvider.UpcomingMedia();

            _cache.Remove(CACHE_KEY);
            _cache.SetIfNotExists(CACHE_KEY, upcoming);
        }

        MediaCard[] media = await GetOrCreateMediaCardsFromIds(upcoming.Keys.ToList());
        return media.Select(x => x.WithReleaseDate(upcoming[x.aniListId])).OrderBy(x => x.nextReleaseDate).Take(take).ToArray();
    }

    public async Task<MediaCard[]> GetOrCreateMediaCardsFromIds(List<int> ids)
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
}

using System.Net.Http.Json;
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

    public static string GetCardCacheId(int id) => $"CardCache_{id}";
    public static string GetInfoCacheId(int id) => $"Info_{id}";

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

        query.allowAdultContent = _settings.getCache.Get(ConfigKeys.AdultContent, false);

        (int total, int[] media) = await _currentProvider.SearchMedia(query);
        pageResponse = new PageResponse<MediaCard>(await GetOrCreateMediaCardsFromIds(media.ToList()), query.page ?? 1, query.pageSize ?? 10, total);

        _cache.Set(CACHE_KEY, pageResponse);
        _cache.Set(SEARCH_CACHE, searchCacheValue);

        return pageResponse;
    }

    public async Task<MediaInfo> GetMediaInfoForUser(UserSession? usr, int aniListId)
    {
        MediaInfo info = await GetMediaInfo(aniListId);

        if (usr != null)
        {
            Model_UserBookmark? bookmark = await _db.userBookmarks.FirstOrDefaultAsync(b => b.UserId == usr.User.Id && b.MediaId == aniListId);
            info.RegisterBookmark(bookmark);
        }

        return info;
    }

    public async Task<MediaInfo> GetMediaInfo(int aniListId)
        => (await GetMediaInfo([aniListId]))[0];

    public async Task<MediaInfo[]> GetMediaInfo(ICollection<int> ids)
    {
        if (ids?.Count == 0)
            return [];

        List<int> remainingIds = new List<int>(ids);
        List<MediaInfo> results = new List<MediaInfo>();

        // find cached version
        long currentTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

        for (int i = remainingIds.Count - 1; i >= 0; i--)
        {
            string cacheId = GetInfoCacheId(remainingIds[i]);

            if (_cache.TryGetValue(cacheId, out MediaInfo? info))
            {
                if (info != null)
                {
                    if (info.upcomingEpisode.HasValue && info.upcomingEpisode.Value < currentTime)
                    {
                        // invalidate cache
                        _cache.Remove(cacheId);
                        _cache.Remove(GetCardCacheId(info.id));
                        continue;
                    }

                    results.Add(info);
                }

                remainingIds.RemoveAt(i);
            }
        }

        if (remainingIds.Count == 0)
            return results.ToArray();

        // create db entries for those that do not exist

        List<int> initialCreates = new List<int>();
        HashSet<int> existingEntriesInDB = await _db.media.Where(m => remainingIds.Contains(m.Id)).Select(m => m.Id).ToHashSetAsync();

        foreach (int desired in remainingIds)
            if (!existingEntriesInDB.Contains(desired))
                initialCreates.Add(desired);

        _ = await GetOrCreateMediaCardsFromIds([.. initialCreates]);

        // hydrate

        Model_Media[] dbEntries = await _db.media
            .Include(m => m.Tags)
            .Include(m => m.Genres)
            .Include(m => m.RelatedMedia)
            .Where(m => remainingIds.Contains(m.Id)).ToArrayAsync();

        Dictionary<int, Model_Tag?> tagLookup = new();
        Dictionary<int, MediaCard?> mediaLookup = new();
        Dictionary<int, Model_Link[]?> linkLookup = new();
        HashSet<int> episodesToRehydrateUpcomingDate = new();

        foreach (Model_Media media in dbEntries)
        {
            remainingIds.Remove(media.Id);
            linkLookup[media.Id] = null;

            foreach (var tag in media.Tags)
                tagLookup[tag.TagId] = null;

            foreach (var relation in media.RelatedMedia)
                mediaLookup[relation.ConnectedMediaId] = null;

            if (media.NextEpisodeReleaseDate.HasValue && media.NextEpisodeReleaseDate < currentTime)
            {
                if (!episodesToRehydrateUpcomingDate.Contains(media.Id))
                    episodesToRehydrateUpcomingDate.Add(media.Id);
            }
        }

        var links = await (
            from l in _db.links

            where l.anilist_id.HasValue
                && l.tvdb_id.HasValue
                && linkLookup.Keys.Contains(l.anilist_id.Value)

            join cl in _db.links
                on l.tvdb_id equals cl.tvdb_id
                into grouped

            select new
            {
                id = l.anilist_id!.Value,
                relatedIds = grouped.Distinct()
            }
        ).ToArrayAsync();

        foreach (var link in links)
        {
            linkLookup[link.id] = link.relatedIds.ToArray();

            foreach (Model_Link linkMediaId in link.relatedIds)
                mediaLookup[linkMediaId.anilist_id!.Value] = null;
        }

        Model_Tag[] tags = await _db.tags.Where(t => tagLookup.Keys.Contains(t.Id)).ToArrayAsync();

        foreach (Model_Tag tag in tags)
            tagLookup[tag.Id] = tag;

        // fetch related content
        MediaCard[] cards = await GetOrCreateMediaCardsFromIds(mediaLookup.Keys.ToList());

        foreach (MediaCard card in cards)
            mediaLookup[card.aniListId] = card;

        var nextEpisodeReleaseDateLookup = await _hydrationService.HydrateReleaseDates(episodesToRehydrateUpcomingDate);

        foreach (Model_Media media in dbEntries)
        {
            MediaCard[] recommend = media.RelatedMedia.Where(r => mediaLookup.ContainsKey(r.ConnectedMediaId)).Select(r => mediaLookup[r.ConnectedMediaId]!).ToArray();
            Model_Tag?[] mediaTags = media.Tags.Select(t =>
            {
                if (tagLookup.TryGetValue(t.TagId, out Model_Tag? tag))
                    return tag;

                return null;
            }).ToArray();

            MediaInfo info = MediaInfo.Map(media).RegisterTags(mediaTags).RegisterRelated(recommend);

            if (linkLookup.TryGetValue(media.Id, out Model_Link[]? linkedMedia) && linkedMedia != null)
            {
                (Model_Link, MediaCard)[] hydratedLinks = linkedMedia.Where(l => mediaLookup.ContainsKey(l.anilist_id!.Value)).Select(l => (l, mediaLookup[l.anilist_id!.Value]!)).ToArray();
                info.RegisterConnectedMedia(hydratedLinks);
            }

            if (nextEpisodeReleaseDateLookup.TryGetValue(info.id, out long? newDate))
                info.upcomingEpisode = newDate;

            results.Add(info);
            _cache.Set(GetInfoCacheId(media.Id), info);
        }

        return results.ToArray();
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

    public async Task<MediaCard[]> GetOrCreateMediaCardsFromIds(List<int> ids, MediaSearchQuery? req = null)
    {
        MediaCard? card;
        List<MediaCard> results = new List<MediaCard>();

        for (int i = ids.Count - 1; i >= 0; i--)
        {
            if (_cache.TryGetValue(GetCardCacheId(ids[i]), out card) && card != null)
            {
                ids.RemoveAt(i);

                if (card.IsInFilter(req))
                    results.Add(card);
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

            if (card.IsInFilter(req))
                results.Add(card);
        }

        IEnumerable<MediaCard> newCards = (await _hydrationService.SaveMedia(ids, req)).Select(MediaCard.Map);

        foreach (MediaCard newCard in newCards)
            _cache.Set(GetCardCacheId(newCard.aniListId), newCard);

        return [.. results, .. newCards];
    }

    public async Task<MediaInfo[]> GetTrending(int limit)
    {
        string cacheKey = $"{nameof(GetTrending)}_{limit}";

        List<int>? ids = [];

        if (!_cache.TryGetValue(cacheKey, out ids))
            ids = await _currentProvider.GetTrending(limit);

        _cache.Set(cacheKey, ids);
        return await GetMediaInfo(ids ?? []);
    }

    public async Task ClearDatabaseCache()
    {
        _db.RemoveRange(await _db.mediaRelations.ToListAsync());
        _db.RemoveRange(await _db.mediaEpisodes.ToListAsync());
        _db.RemoveRange(await _db.mediaGenres.ToListAsync());
        _db.RemoveRange(await _db.mediaTags.ToListAsync());
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
        string[] tags = await _db.tags.Select(t => t.Name ?? "").ToArrayAsync();

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

    public async Task CheckForOutOfDateEpisodes()
    {
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        int[] watchingBookmarks = [
            (int)BookmarkType.Watching,
            (int)BookmarkType.OnHold,
            (int)BookmarkType.Planning,
        ];

        Model_UserBookmark[] bookmarks = await _db.userBookmarks.Where(m => watchingBookmarks.Contains(m.BookmarkId)).ToArrayAsync();

        List<int> distinctMedia = new List<int>();
        Dictionary<int, List<Guid>> userMediaLookup = new Dictionary<int, List<Guid>>();

        foreach (Model_UserBookmark bookmark in bookmarks)
        {
            if (!userMediaLookup.TryGetValue(bookmark.MediaId, out var list))
            {
                list = new List<Guid>();
                userMediaLookup[bookmark.MediaId] = list;
                distinctMedia.Add(bookmark.MediaId);
            }

            list.Add(bookmark.UserId);
        }

        MediaCard[] mediaCards = await GetOrCreateMediaCardsFromIds(distinctMedia);

        List<int> mediaOfInterest = mediaCards.Where(m => m.nextReleaseDate.HasValue && m.nextReleaseDate < currentTime).Select(m => m.aniListId).ToList();

        Dictionary<int, long?> newEpisodes = await _currentProvider.GetTimeOfNextEpisodes(mediaOfInterest);
        Dictionary<int, Model_Media> dbEntries = await _db.media.Where(m => mediaOfInterest.Contains(m.Id)).ToDictionaryAsync(m => m.Id, m => m);

        List<Model_Notification> newNotifications = new List<Model_Notification>();

        Model_Media? media = null;
        List<Guid>? users = null;

        foreach (KeyValuePair<int, long?> episodeUpdate in newEpisodes)
        {
            if (dbEntries.TryGetValue(episodeUpdate.Key, out media) && media != null)
            {
                _cache.Remove(GetCardCacheId(media.Id));
                _cache.Remove(GetInfoCacheId(media.Id));

                media.NextEpisodeReleaseDate = episodeUpdate.Value;

                if (userMediaLookup.TryGetValue(episodeUpdate.Key, out users) && users?.Count > 0)
                {
                    foreach (Guid usr in users)
                        newNotifications.Add(new Model_Notification()
                        {
                            Date = DateTime.UtcNow,
                            EventName = "New Episode",
                            Message = "New Episode",

                            MediaId = media.Id,
                            MediaEpisode = media.EpisodeCount + 1,

                            UserId = usr,
                        });
                }
            }
        }

        if (newNotifications.Count > 0)
            await _db.AddRangeAsync(newNotifications);

        await _db.SaveChangesAsync();
    }
}

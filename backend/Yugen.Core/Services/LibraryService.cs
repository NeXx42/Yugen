using EFCore.BulkExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Yugen.Core.Data;
using Yugen.Core.Factories;
using Yugen.Core.Helpers;
using Yugen.Data;
using Yugen.Domain.Data;
using Yugen.Domain.Data.Downloads;
using Yugen.Domain.Data.Media;
using Yugen.Domain.Data.Users;
using Yugen.Domain.Enums;
using Yugen.Domain.Models.Bookmarks;
using Yugen.Domain.Models.History;
using Yugen.Domain.Models.Library;
using Yugen.Domain.Models.Linking;
using Yugen.Domain.Models.Media;
using Yugen.Providers;
using Yugen.Providers.Radarr;
using Yugen.Providers.Sonarr;

namespace Yugen.Core.Services;

public class LibraryService
{
    private readonly YugenContext _db;

    private readonly CacheService _cache;
    private readonly MediaService _mediaService;
    private readonly CatalogService _catalogService;
    private readonly HydrationService _hydrationService;

    private readonly EndpointDeduplicator _endpointDeduplicator;

    private readonly LibraryFactory _library;

    public LibraryService(YugenContext db,
                        SettingsCache settings,
                        CatalogService catalogService,
                        MediaService mediaService,
                        CacheService cache,
                        HydrationService hydrationService,
                        EndpointDeduplicator endpointDeduplicator)
    {
        _db = db;

        _cache = cache;
        _mediaService = mediaService;
        _catalogService = catalogService;
        _hydrationService = hydrationService;

        _endpointDeduplicator = endpointDeduplicator;

        _library = new LibraryFactory(
            new SonarrLibraryProvider(settings.Get(ConfigKeys.Sonarr_Url), settings.Get(ConfigKeys.Sonarr_ApiKey)),
            new RadarrLibraryProvider(settings.Get(ConfigKeys.Radarr_Url), settings.Get(ConfigKeys.Radarr_ApiKey))
        );
    }

    public async Task<DownloadedEpisode[]> GetDownloadedEpisodes(UserSession usr, int aniListId)
    {
        Model_DownloadedMedia? media = await RecheckDownloads(usr, aniListId);

        if (media == null)
            return [];

        return media.downloadedEpisodes.Select(DownloadedEpisode.Map).ToArray();
    }

    public async Task<Model_DownloadedMedia?> RecheckDownloads(UserSession usr, int aniListId, bool force = false)
    {
        using var concurrentCheck = _endpointDeduplicator.TryAcquire(usr, nameof(RecheckDownloads), aniListId.ToString());

        Model_Link? link = await _db.links.FirstOrDefaultAsync(l => l.anilist_id == aniListId);

        if (link == null)
            return null;

        Model_DownloadedMedia? downloadedMedia = await _db.downloadedMedia.FirstOrDefaultAsync(d => d.MediaId == aniListId);

        if (downloadedMedia != null)
        {
            try
            {
                _db.Remove(downloadedMedia);
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return null;
            }
        }

        downloadedMedia = await _library.GetFactory(link!).GetDownloadedEpisodes(aniListId, link!);

        if (downloadedMedia == null)
            return null;

        string?[]? jellyfinIds = await _mediaService.GetJellyfinIdsForEpisodes(downloadedMedia.downloadedEpisodes);

        if (jellyfinIds != null)
            for (int i = 0; i < jellyfinIds.Length; i++)
                downloadedMedia.downloadedEpisodes.ElementAt(i).JellyfinId = jellyfinIds[i];

        await _db.AddAsync(downloadedMedia);
        await _db.SaveChangesAsync();

        return downloadedMedia;
    }

    public async Task<PageResponse<MediaCard>> GetWatchHistory(int page, int pageSize)
    {
        var query = _db.watchHistory
            .Where(w => w.WatchedEpisode.HasValue)
            .Select(w => new
            {
                w.MediaId,
                Episode = w.WatchedEpisodes.FirstOrDefault(e => e.EpisodeNumber == w.WatchedEpisode)
            })
            .Where(x => x.Episode != null)
            .OrderByDescending(x => x.Episode!.LastWatched);

        int totalResults = await query.CountAsync();
        var results = await query.Skip(page * pageSize).Take(pageSize).ToArrayAsync();

        Dictionary<int, Model_WatchedEpisode?> historyLookup = results.ToDictionary(x => x.MediaId, x => x.Episode);
        MediaCard[] cards = await _catalogService.GetOrCreateMediaCardsFromIds(results.Select(r => r.MediaId).ToList());

        return new PageResponse<MediaCard>(cards.Select(c => c.WithWatchInfo(historyLookup[c.aniListId])).ToArray(), page, pageSize, totalResults);
    }

    public async Task SyncWatchHistory(UserSession usr)
    {
        using var concurrentCheck = _endpointDeduplicator.TryAcquire(usr, nameof(SyncWatchHistory));

        int[] downloadedMedia = await _db.downloadedMedia.Select(m => m.MediaId).ToArrayAsync();

        foreach (int i in downloadedMedia)
            await _mediaService.SyncWatchHistoryWithJellyfin(usr, i, true);
    }

    public async Task<int?> ResyncLibrary(UserSession usr)
    {
        using var concurrentCheck = _endpointDeduplicator.TryAcquire(usr, nameof(ResyncLibrary));

        List<int>? ids = await _library.GetFactory().GetDownloadedMedia();

        if (ids == null)
            return null;

        _db.sonarrEpisodes.RemoveRange(_db.sonarrEpisodes);
        _db.downloadedMedia.RemoveRange(_db.downloadedMedia);
        await _db.SaveChangesAsync();

        int importCount = 0;
        List<int?> links = await _db.links.Where(l => ids.Contains(l.tvdb_id ?? -1)).Select(l => l.anilist_id).ToListAsync();

        foreach (int? link in links)
        {
            if (link == null)
                continue;

            importCount++;
            await RecheckDownloads(usr, link.Value, true);
        }

        return importCount;
    }

    public async Task<EpisodeInfo?> GetFilmEpisodeContainer(UserSession usr, int aniListId, bool refetch)
    {
        Model_DownloadedMedia? media = await _db.downloadedMedia.Include(m => m.downloadedEpisodes).FirstOrDefaultAsync(m => m.MediaId == aniListId);

        if ((media?.ProviderType ?? LibraryProviderType.Sonarr) != LibraryProviderType.Radarr)
            throw new Exception("Cannot get film from non Radarr source");

        if (media?.downloadedEpisodes.Count != 1)
            return null;

        Model_WatchedEpisode? history = await _db.watchedEpisodes.FirstOrDefaultAsync(w => w.MediaId == aniListId && w.EpisodeNumber == media.downloadedEpisodes.ElementAt(0).EpisodeNumber);
        return EpisodeInfo.Map(null, media.downloadedEpisodes.ElementAt(0), history);
    }

    public async Task<EpisodeInfo[]> GetMediaEpisodesForUser(UserSession usr, int aniListId, bool refetch)
    {
        Model_Media? media = await _db.media.FirstOrDefaultAsync(m => m.Id == aniListId);

        if (media == null)
            return [];

        if (refetch || !(media.Hydrated ?? false))
        {
            await _hydrationService.HydrateEpisodes(media);

            await RecheckDownloads(usr, aniListId, true);
            await _mediaService.SyncWatchHistoryWithJellyfin(usr, aniListId, true);
        }

        var results = await (
            from m in _db.mediaEpisodes

            join de in _db.sonarrEpisodes
                on new { m.MediaId, m.EpisodeNumber }
                equals new { de.MediaId, de.EpisodeNumber }
                into downloads
            from de in downloads.DefaultIfEmpty()

            join wh in _db.watchedEpisodes
                on new { m.MediaId, m.EpisodeNumber }
                equals new { wh.MediaId, wh.EpisodeNumber }
                into watch
            from wh in watch.DefaultIfEmpty()

            where m.MediaId == aniListId

            select new
            {
                Episode = m,
                DownloadData = de,
                WatchHistory = wh,
            }
        ).ToArrayAsync();

        return results.Select(r => EpisodeInfo.Map(r.Episode, r.DownloadData, r.WatchHistory)).ToArray();
    }

    public async Task<PageResponse<MediaCard>> SearchLibrary(UserSession session, int page, int pageSize, string group)
    {
        IQueryable<int>? query = null;

        switch (group.ToLower())
        {
            case "downloaded":
                query = _db.downloadedMedia.Include(m => m.downloadedEpisodes).Where(m => m.downloadedEpisodes.Any(e => e.fileId.HasValue)).Select(m => m.MediaId);
                break;

            default:

                if (Enum.TryParse(group, out BookmarkType bookmarkType))
                {
                    query = _db.userBookmarks.Where(b => b.UserId == session.User.Id && b.BookmarkId == (int)bookmarkType).Select(b => b.MediaId);
                }

                break;
        }

        if (query == null)
            return PageResponse<MediaCard>.Empty();

        int totalCount = await query.CountAsync();
        List<int> results = await query.Skip(page * pageSize).Take(pageSize).ToListAsync();

        return new PageResponse<MediaCard>((await _catalogService.GetOrCreateMediaCardsFromIds(results)).OrderBy(c => c.Title).ToArray(), page, pageSize, totalCount);
    }

    public async Task UpdateBookmark(UserSession usr, int mediaId, int bookmarkId)
    {
        using var concurrentCheck = _endpointDeduplicator.TryAcquire(usr, nameof(UpdateBookmark), mediaId.ToString());

        _db.RemoveRange(_db.userBookmarks.Where(b => b.UserId == usr.User.Id && b.MediaId == mediaId));

        if (bookmarkId <= 0 || bookmarkId > (int)BookmarkType.Dropped)
            return;

        await _db.AddAsync(new Model_UserBookmark()
        {
            MediaId = mediaId,
            UserId = usr.User.Id,
            BookmarkId = bookmarkId,
            DateAdded = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync();
    }

    public async Task UploadLibrary(UserSession usr, IFormFile file)
    {
        using var concurrentCheck = _endpointDeduplicator.TryAcquire(usr, nameof(UploadLibrary));

        DateTime dateAdded = DateTime.UtcNow;
        List<Model_UserBookmark> allBookmarks = new List<Model_UserBookmark>();

        BookmarkType? currentGroupHeader = null;
        List<int> currentGroup = new List<int>();

        using (StreamReader reader = new StreamReader(file.OpenReadStream()))
        {
            string? line;

            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (line.StartsWith("### "))
                {
                    if (currentGroupHeader.HasValue)
                    {
                        allBookmarks.AddRange(currentGroup.Select(i => new Model_UserBookmark()
                        {
                            UserId = usr.User.Id,
                            MediaId = i,
                            BookmarkId = (int)currentGroupHeader.Value,
                            DateAdded = dateAdded,
                        }));

                        currentGroup.Clear();
                    }


                    string headerStr = line.Substring(4, line.Length - 4).Replace("-", "");
                    if (Enum.TryParse(headerStr, out BookmarkType header))
                    {
                        currentGroupHeader = header;
                        continue;
                    }
                    else
                    {
                        throw new Exception($"Unknown header group - {headerStr}");
                    }
                }
                else if (line.StartsWith("# "))
                {
                    _ = await reader.ReadLineAsync(); // mal
                    string? aniList = await reader.ReadLineAsync();

                    if (string.IsNullOrEmpty(aniList))
                        continue;

                    aniList = aniList.Replace("https://anilist.co/anime/", "").Replace("/", "");

                    if (int.TryParse(aniList, out int id))
                    {
                        currentGroup.Add(id);
                    }
                }
            }
        }

        if (currentGroupHeader.HasValue)
        {
            allBookmarks.AddRange(currentGroup.Select(i => new Model_UserBookmark()
            {
                UserId = usr.User.Id,
                MediaId = i,
                BookmarkId = (int)currentGroupHeader.Value,
                DateAdded = dateAdded,
            }));
        }

        _db.RemoveRange(_db.userBookmarks.Where(b => b.UserId == usr.User.Id));
        await _db.SaveChangesAsync();
        await _db.BulkInsertAsync(allBookmarks);
    }

    public async Task<bool> RequestSeries(UserSession usr, int mediaId, DownloadRequest request)
    {
        using var concurrentCheck = _endpointDeduplicator.TryAcquire(usr, nameof(RequestSeries), mediaId.ToString());

        Model_DownloadedMedia? existingDownload = await _db.downloadedMedia.FirstOrDefaultAsync(m => m.MediaId == mediaId);
        Model_DownloadedMedia? newDownload = await _library.GetFactory((LibraryProviderType)request.libraryProvider).RequestSeries(mediaId, existingDownload, request);

        if (newDownload == null)
            return false;

        if (existingDownload == null)
        {
            await _db.AddAsync(newDownload);
            await _db.SaveChangesAsync();

            return true;
        }

        _db.Remove(existingDownload);
        await _db.AddAsync(newDownload);
        await _db.SaveChangesAsync();

        return false;
    }

    public async Task ResearchDownloads(UserSession usr, int aniListId)
    {
        using var concurrentCheck = _endpointDeduplicator.TryAcquire(usr, nameof(GetSeriesRequestInfo), aniListId.ToString());

        Model_DownloadedMedia? media = await _db.downloadedMedia.FirstOrDefaultAsync(d => d.MediaId == aniListId);
        if (media != null) await _library.GetFactory(media).ResearchMedia(media);
    }

    public async Task<DownloadRequestInfo> GetSeriesRequestInfo(UserSession usr, int aniListId)
    {
        using var concurrentCheck = _endpointDeduplicator.TryAcquire(usr, nameof(GetSeriesRequestInfo), aniListId.ToString());

        Model_DownloadedMedia? existingData = await _db.downloadedMedia.Include(d => d.downloadedEpisodes).FirstOrDefaultAsync(d => d.MediaId == aniListId);

        Model_Link? link = await _db.links.FirstOrDefaultAsync(l => l.anilist_id == aniListId);
        DownloadRequestInfo requestInfo = await _library.GetFactory(link!).GetRequestInfo(link!);

        if (existingData == null)
            existingData = await RecheckDownloads(usr, aniListId);

        if (existingData == null)
            return requestInfo;

        requestInfo.monitored = existingData.IsMonitored;
        requestInfo.downloadedEpisodes = existingData.downloadedEpisodes.Select(e => new DownloadRequestInfo.Episode()
        {
            providerId = e.Id,
            episodeNumber = e.EpisodeNumber,
            monitored = e.monitored,

            jellyfinId = e.JellyfinId
        }).ToArray();

        for (int i = 0; i < requestInfo.qualities.Length; i++)
            if (requestInfo.qualities[i].id == existingData.ExternalQuality)
            {
                requestInfo.selectedQuality = i;
                break;
            }

        for (int i = 0; i < requestInfo.roots.Length; i++)
            if (requestInfo.roots[i].path == existingData.ExternalRoot)
            {
                requestInfo.selectedRoot = i;
                break;
            }

        return requestInfo;
    }

    public async Task DeleteMedia(UserSession usr, int aniListId)
    {
        using var concurrentCheck = _endpointDeduplicator.TryAcquire(usr, nameof(DeleteMedia), aniListId.ToString());

        Model_DownloadedMedia? media = await RecheckDownloads(usr, aniListId, true);

        if (media != null)
        {
            if (media.downloadedEpisodes.Any(e => e.monitored))
                throw new Exception("Cannot delete with monitored episodes");

            await _library.GetFactory(media).DeleteMedia(media);
            await RecheckDownloads(usr, aniListId, true);
        }
    }
}

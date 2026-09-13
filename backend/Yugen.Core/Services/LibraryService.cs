using EFCore.BulkExtensions;
using Microsoft.AspNetCore.Http;
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
using Yugen.Domain.Interfaces;
using Yugen.Domain.Models.Bookmarks;
using Yugen.Domain.Models.History;
using Yugen.Domain.Models.Library;
using Yugen.Domain.Models.Linking;
using Yugen.Domain.Models.Media;
using Yugen.Providers;

namespace Yugen.Core.Services;

public class LibraryService
{
    private readonly YugenContext _db;

    private readonly CacheService _cache;
    private readonly MediaService _mediaService;
    private readonly CatalogService _catalogService;
    private readonly HydrationService _hydrationService;
    private readonly ILogging _loggingService;

    private readonly EndpointDeduplicator _endpointDeduplicator;

    private readonly LibraryFactory _library;

    public LibraryService(YugenContext db,
                        SettingsCache settings,
                        CatalogService catalogService,
                        MediaService mediaService,
                        CacheService cache,
                        HydrationService hydrationService,
                        EndpointDeduplicator endpointDeduplicator,
                        ILogging loggingService)
    {
        _db = db;

        _cache = cache;
        _mediaService = mediaService;
        _catalogService = catalogService;
        _loggingService = loggingService;
        _hydrationService = hydrationService;

        _endpointDeduplicator = endpointDeduplicator;

        _library = LibraryFactory.Create(settings, loggingService);
    }

    public async Task<Model_DownloadedMedia?> RecheckDownloads(UserSession usr, int mediaId, bool force = false)
    {
        using var concurrentCheck = _endpointDeduplicator.TryAcquire(usr, nameof(RecheckDownloads), mediaId.ToString());

        Model_Link link = await _db.links.SingleAsync(l => l.MediaId == mediaId);
        Model_DownloadedMedia? downloadedMedia = await _db.downloadedMedia.FirstOrDefaultAsync(d => d.MediaId == mediaId);

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

        Model_Media? media = await _db.media.FirstOrDefaultAsync(m => m.Id == mediaId);

        if (media == null)
            return null;

        ILibraryProvider lib = _library.GetFactory(media);

        if (!lib.isSetup)
            return null;

        downloadedMedia = await lib.GetDownloadedEpisodes(mediaId, link!);

        if (downloadedMedia == null)
            return null;

        string?[]? jellyfinIds = await _mediaService.GetJellyfinIdsForEpisodes(usr, downloadedMedia.downloadedEpisodes);

        if (jellyfinIds != null)
            for (int i = 0; i < jellyfinIds.Length; i++)
                downloadedMedia.downloadedEpisodes.ElementAt(i).JellyfinId = jellyfinIds[i];

        await _db.AddAsync(downloadedMedia);
        await _db.SaveChangesAsync();

        return downloadedMedia;
    }

    public async Task<int?> ResyncLibrary(UserSession usr)
    {
        using var concurrentCheck = _endpointDeduplicator.TryAcquire(usr, nameof(ResyncLibrary));

        List<int> tvdbIds = new List<int>();
        List<int> tmdbIds = new List<int>();

        foreach (ILibraryProvider provider in _library.GetFactories())
        {
            if (!provider.isSetup)
                continue;

            (string linkId, List<int> ids)? res = await provider.GetDownloadedMedia();

            if (res.HasValue)
            {
                switch (res.Value.linkId)
                {
                    case nameof(Model_Link.tvdb_id):
                        tvdbIds.AddRange(res.Value.ids);
                        break;

                    case nameof(Model_Link.themoviedb_id):
                        tmdbIds.AddRange(res.Value.ids);
                        break;
                }

            }
        }

        if (tvdbIds.Count + tmdbIds.Count == 0)
            return null;

        _db.downloadedMedia.RemoveRange(_db.downloadedMedia.Include(e => e.downloadedEpisodes));
        await _db.SaveChangesAsync();

        int importCount = 0;
        int[] links = await _db.links
            .Where(l =>
                (l.themoviedb_id.HasValue && tmdbIds.Contains(l.themoviedb_id.Value)) ||
                (l.tvdb_id.HasValue && tvdbIds.Contains(l.tvdb_id.Value))
            )
            .Select(l => l.MediaId)
            .ToArrayAsync();

        foreach (int link in links)
        {
            try
            {
                await RecheckDownloads(usr, link, true);
                importCount++;
            }
            catch { }
        }

        return importCount;
    }

    public async Task<EpisodeInfo?> GetFilmEpisodeContainer(UserSession? usr, int mediaId, bool refetch)
    {
        Model_DownloadedMedia? media = await _db.downloadedMedia.Include(m => m.downloadedEpisodes).FirstOrDefaultAsync(m => m.MediaId == mediaId);

        if (media?.downloadedEpisodes.Count != 1)
            return null;

        if (media.ProviderType != LibraryProviderType.Radarr)
            throw new Exception("Cannot get film from non Radarr source");

        Model_WatchHistory? history = null;

        if (usr != null)
            history = await _db.watchHistory.Include(w => w.WatchedEpisodes).FirstOrDefaultAsync(w => w.UserId == usr.User.Id && w.MediaId == mediaId);

        return EpisodeInfo.Map((null, media.downloadedEpisodes.ElementAt(0), history?.WatchedEpisodes.FirstOrDefault()));
    }

    public async Task<EpisodeInfo[]> GetMediaEpisodesForUser(UserSession? usr, int mediaId, bool refetch, bool clearOld)
    {
        Model_Media? media = await _db.media.FirstOrDefaultAsync(m => m.Id == mediaId);

        if (media == null)
            return [];

        if (refetch || media.Episodes.Count == 0)
        {
            try
            {
                await _hydrationService.HydrateEpisodes(media, clearOld);
            }
            catch (Exception e)
            {
                _loggingService.LogError(e);

                if (refetch)
                    throw;
            }

            if (usr != null) // jellyfin's api doesnt return all results without the userid??
                await RecheckDownloads(usr, mediaId, true);
        }

        List<Model_MediaEpisode> episodeMetadata = await _db.mediaEpisodes.Where(e => e.MediaId == mediaId).OrderBy(e => e.EpisodeNumber).ToListAsync();
        List<Model_DownloadedEpisode> downloadMetadata = await _db.downloadedEpisodes.Where(e => e.MediaId == mediaId).OrderBy(e => e.EpisodeNumber).ToListAsync();

        Dictionary<int, (Model_MediaEpisode? metaData, Model_DownloadedEpisode? downloadData, Model_WatchedEpisode? watchData)>
            episodes = episodeMetadata.ToDictionary(e => e.EpisodeNumber, e => (metaData: e, downloadData: (Model_DownloadedEpisode?)null, watchData: (Model_WatchedEpisode?)null))!;

        foreach (Model_DownloadedEpisode download in downloadMetadata)
        {
            if (episodes.TryGetValue(download.EpisodeNumber, out var existing))
            {
                episodes[download.EpisodeNumber] = (existing.metaData, download, existing.watchData);
            }
            else
            {
                episodes.Add(download.EpisodeNumber, (null, download, null));
            }
        }

        Model_WatchHistory? history = null;

        if (usr != null)
        {
            history = await _db.watchHistory.Include(w => w.WatchedEpisodes).FirstOrDefaultAsync(w => w.UserId == usr.User.Id && w.MediaId == mediaId); ;

            foreach (Model_WatchedEpisode watch in history?.WatchedEpisodes ?? [])
            {
                if (episodes.TryGetValue(watch.EpisodeNumber, out var ep))
                    episodes[watch.EpisodeNumber] = (ep.metaData, ep.downloadData, watch);
            }
        }

        return episodes.Values.Select(EpisodeInfo.Map).ToArray();
    }

    public async Task<PageResponse<MediaCard>> SearchLibrary(UserSession session, MediaSearchQuery? req, string group)
    {
        IQueryable<int>? query = null;

        switch (group.ToLower())
        {
            case "continuewatching":
                return await GetWatchHistory(session, req);

            case "downloaded":
                query = _db.downloadedMedia.Include(m => m.downloadedEpisodes)
                    .Where(m => m.downloadedEpisodes.Any(e => e.fileId.HasValue))
                    .OrderByDescending(m => m.MediaId)
                    .Select(m => m.MediaId);
                break;

            default:

                if (Enum.TryParse(group, out BookmarkType bookmarkType))
                {
                    query = _db.userBookmarks
                        .Where(b => b.UserId == session.User.Id && b.BookmarkId == (int)bookmarkType)
                        .OrderByDescending(b => b.DateAdded)
                        .Select(b => b.MediaId);
                }

                break;
        }

        if (query == null)
            return PageResponse<MediaCard>.Empty();

        int totalResults = await query.CountAsync();

        int page = Math.Max(req?.page ?? 1, 1);
        int pageSize = req?.pageSize ?? 10;

        List<int> ids = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        MediaCard[] cards = await _catalogService.GetOrCreateMediaCardsFromIds(ids);

        return new PageResponse<MediaCard>(cards, page, pageSize, totalResults);
    }

    public async Task<PageResponse<MediaCard>> GetWatchHistory(UserSession usr, MediaSearchQuery? req)
    {
        int page = Math.Max(req?.page ?? 1, 1);
        int pageSize = req?.pageSize ?? 10;

        var query = _db.watchHistory
            .Include(w => w.WatchedEpisodes)
            .Where(w => w.UserId == usr.User.Id && w.LastWatchedEpisodeNumber.HasValue)
            .OrderByDescending(w => w.UpdatedTime)
            .Select(w => new
            {
                Media = w,
                Episode = w.WatchedEpisodes.FirstOrDefault(e => e.EpisodeNumber == w.LastWatchedEpisodeNumber)
            })
            .Where(x => x.Episode != null);

        int totalCount = await query.CountAsync();
        var history = await query.Skip((page - 1) * pageSize).Take(pageSize).ToArrayAsync();

        List<int> mediaIds = history.Select(m => m.Media.MediaId).ToList();
        MediaCard[] cards = await _catalogService.GetOrCreateMediaCardsFromIds(mediaIds);

        foreach (var fullHistory in history)
            cards.FirstOrDefault(c => c.aniListId == fullHistory.Media.MediaId)?.WithWatchInfo(fullHistory.Media, fullHistory.Episode);

        return new PageResponse<MediaCard>(cards, page, pageSize, totalCount);
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

        _db.RemoveRange(existingDownload.downloadedEpisodes);
        _db.Remove(existingDownload);
        await _db.SaveChangesAsync();

        await _db.AddAsync(newDownload);
        await _db.SaveChangesAsync();

        return false;
    }

    public async Task ResearchDownloads(UserSession usr, int mediaId)
    {
        using var concurrentCheck = _endpointDeduplicator.TryAcquire(usr, nameof(GetSeriesRequestInfo), mediaId.ToString());

        Model_DownloadedMedia? media = await _db.downloadedMedia.FirstOrDefaultAsync(d => d.MediaId == mediaId);
        if (media != null) await _library.GetFactory(media).ResearchMedia(media);
    }

    public async Task<DownloadRequestInfo> GetSeriesRequestInfo(UserSession usr, int mediaId)
    {
        using var concurrentCheck = _endpointDeduplicator.TryAcquire(usr, nameof(GetSeriesRequestInfo), mediaId.ToString());

        Model_DownloadedMedia? existingData = await _db.downloadedMedia.Include(d => d.downloadedEpisodes).FirstOrDefaultAsync(d => d.MediaId == mediaId);

        Model_Media media = await _db.media
            .Include(m => m.link)
            .SingleAsync(m => m.Id == mediaId);
        DownloadRequestInfo requestInfo = await _library.GetFactory(media!).GetRequestInfo(media.link);

        if (existingData == null)
            existingData = await RecheckDownloads(usr, mediaId);

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

    public async Task DeleteMedia(UserSession usr, int mediaId)
    {
        using var concurrentCheck = _endpointDeduplicator.TryAcquire(usr, nameof(DeleteMedia), mediaId.ToString());

        Model_DownloadedMedia? media = await RecheckDownloads(usr, mediaId, true);

        if (media != null)
        {
            if (media.downloadedEpisodes.Any(e => e.monitored))
                throw new Exception("Cannot delete with monitored episodes");

            await _library.GetFactory(media).DeleteMedia(media);
            await RecheckDownloads(usr, mediaId, true);
        }
    }

    public async Task ClearMediaHistory(UserSession usr, int mediaId)
    {
        _db.RemoveRange(_db.watchHistory.Include(h => h.WatchedEpisodes).Where(e => e.UserId == usr.User.Id && e.MediaId == mediaId));

        await _db.SaveChangesAsync();

        _cache.Remove(CatalogService.GetCardCacheId(mediaId));
        _cache.Remove(CatalogService.GetInfoCacheId(mediaId));
    }
}

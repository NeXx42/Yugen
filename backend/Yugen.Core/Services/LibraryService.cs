using Azure;
using EFCore.BulkExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Yugen.Core.Data;
using Yugen.Data;
using Yugen.Domain.Data;
using Yugen.Domain.Data.Downloads;
using Yugen.Domain.Data.History;
using Yugen.Domain.Data.Users;
using Yugen.Domain.Enums;
using Yugen.Domain.Models.Bookmarks;
using Yugen.Domain.Models.History;
using Yugen.Domain.Models.Library;
using Yugen.Domain.Models.Linking;
using Yugen.Providers;
using Yugen.Providers.Sonarr;

namespace Yugen.Core.Services;

public class LibraryService
{
    private readonly YugenContext _db;

    private readonly CacheService _cache;
    private readonly MediaService _mediaService;
    private readonly CatalogService _catalogService;

    private readonly ILibraryProvider _libraryProvider;

    public LibraryService(YugenContext db, SettingsCache settings, CatalogService catalogService, MediaService mediaService, CacheService cache)
    {
        _db = db;

        _cache = cache;
        _mediaService = mediaService;
        _catalogService = catalogService;

        _libraryProvider = new SonarrLibraryProvider(settings.Get(ConfigKeys.Sonarr_Url), settings.Get(ConfigKeys.Sonarr_ApiKey));
    }

    public async Task<DownloadedEpisode[]> GetDownloadedEpisodes(int aniListId)
    {
        Model_DownloadedMedia? media = await RecheckDownloads(aniListId);

        if (media == null)
            return [];

        return media.downloadedEpisodes.Select(DownloadedEpisode.Map).ToArray();
    }

    public async Task<Model_DownloadedMedia?> RecheckDownloads(int aniListId, bool force = false)
    {
        Model_Link? link = await _db.links.FirstOrDefaultAsync(l => l.anilist_id == aniListId);

        if (link == null)
            return null;

        Model_DownloadedMedia? media = await _db.downloadedMedia.Include(m => m.downloadedEpisodes).FirstOrDefaultAsync(m => m.MediaId == aniListId);
        Model_DownloadedEpisode[]? episodes = await _libraryProvider.GetDownloadedEpisodes(aniListId, link!);

        if ((episodes?.Length ?? 0) == 0)
        {
            if (media != null)
            {
                _db.RemoveRange(media.downloadedEpisodes);
                _db.Remove(media);

                await _db.SaveChangesAsync();
            }

            return null;
        }

        if (media == null)
        {
            media = new Model_DownloadedMedia()
            {
                MediaId = aniListId
            };

            await _db.downloadedMedia.AddAsync(media);
        }
        else if (!force)
            return media;

        media.LastChecked = DateTime.UtcNow;

        _db.sonarrEpisodes.RemoveRange(media.downloadedEpisodes);

        media.downloadedEpisodes.Clear();
        media.downloadedEpisodes = episodes!;

        await _mediaService.LinkSonarrToJellyfin(media);
        await _db.SaveChangesAsync();

        return media;
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
        int[] downloadedMedia = await _db.downloadedMedia.Select(m => m.MediaId).ToArrayAsync();

        foreach (int i in downloadedMedia)
            await _mediaService.SyncWatchHistoryWithJellyfin(usr, i, true);
    }

    public async Task ResyncLibrary(UserSession __)
    {
        List<int>? ids = await _libraryProvider.GetDownloadedMedia();

        if (ids == null)
            return;

        _db.sonarrEpisodes.RemoveRange(_db.sonarrEpisodes);
        _db.downloadedMedia.RemoveRange(_db.downloadedMedia);
        await _db.SaveChangesAsync();

        List<int> links = await _db.links.Where(l => l.tvdb_id.HasValue && l.anilist_id.HasValue).Where(l => ids.Contains(l.tvdb_id!.Value)).Select(l => l.anilist_id!.Value).ToListAsync();

        // the recheck downloads doesnt fetch previously unseen media
        _ = await _catalogService.GetOrCreateMediaCardsFromIds(links);

        foreach (int link in links)
            await RecheckDownloads(link, true);
    }

    public async Task<WatchHistoryContainer?> GetEpisodeWatchHistory(UserSession _, int seriesId)
    {
        // this should be scoped to user...
        Model_WatchHistory? history = await _db.watchHistory.Include(w => w.WatchedEpisodes).FirstOrDefaultAsync(m => m.MediaId == seriesId);

        if (history == null)
            return null;

        return new WatchHistoryContainer()
        {
            lastWatchedEpisode = history.WatchedEpisode,
            episodes = history.WatchedEpisodes.Select(EpisodeHistory.Map).ToArray()
        };
    }

    public async Task<PageResponse<MediaCard>> SearchLibrary(UserSession session, int page, int pageSize, string group)
    {
        IQueryable<int>? query = null;

        switch (group.ToLower())
        {
            case "downloaded":
                query = _db.downloadedMedia.Select(m => m.MediaId);
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

        return new PageResponse<MediaCard>(await _catalogService.GetOrCreateMediaCardsFromIds(results), page, pageSize, totalCount);
    }

    public async Task UpdateBookmark(UserSession usr, int mediaId, int bookmarkId)
    {
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
}

using System.Net.NetworkInformation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Yugen.Core.Configs;
using Yugen.Core.Data;
using Yugen.Data;
using Yugen.Domain.Data.Downloads;
using Yugen.Domain.Data.Media;
using Yugen.Domain.Data.Users;
using Yugen.Domain.Enums;
using Yugen.Domain.Models;
using Yugen.Domain.Models.Library;
using Yugen.Domain.Models.Linking;
using Yugen.Domain.Models.Media;
using Yugen.Providers;
using Yugen.Providers.Jellyfin;
using Yugen.Providers.Sonarr;

namespace Yugen.Core.Services;

public class LibraryService
{
    private readonly YugenContext _db;

    private readonly MediaService _mediaService;
    private readonly CatalogService _catalogService;

    private readonly ILibraryProvider _libraryProvider;

    public LibraryService(YugenContext db, IOptions<ProviderConfig> options, CatalogService catalogService, MediaService mediaService)
    {
        _db = db;

        _mediaService = mediaService;
        _catalogService = catalogService;

        _libraryProvider = new SonarrLibraryProvider(options.Value.sonarr_Url!, options.Value.sonarr_ApiKey!);
    }

    public async Task ResyncLibrary(UserSession user)
    {

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
        Model_DownloadedMedia? media = await _db.downloadedMedia.Include(m => m.downloadedEpisodes).FirstOrDefaultAsync(m => m.MediaId == aniListId);

        if (media == null)
        {
            media = new Model_DownloadedMedia()
            {
                MediaId = aniListId
            };

            MediaInfo info = await _catalogService.GetMediaInfo(aniListId);
            await _db.downloadedMedia.AddAsync(media);
        }
        else if (!force)
            return media;

        media.LastChecked = DateTime.UtcNow;
        Model_Link? link = await _db.links.FirstOrDefaultAsync(l => l.anilist_id == media.MediaId);

        if (link == null)
        {
            await _db.SaveChangesAsync();
            return null;
        }

        _db.sonarrEpisodes.RemoveRange(media.downloadedEpisodes);

        media.downloadedEpisodes.Clear();
        media.downloadedEpisodes = (await _libraryProvider.GetDownloadedEpisodes(aniListId, link!)) ?? [];

        await _mediaService.LinkSonarrToJellyfin(media);

        await _db.SaveChangesAsync();
        return media;
    }

    public async Task<MediaCard[]> GetWatchHistory(int take)
    {
        List<int> results = await _db.watchHistory
            .Where(w => w.WatchedEpisode.HasValue)
            .Select(w => new
            {
                Id = w.MediaId,
                Episode = w.WatchedEpisodes
                    .FirstOrDefault(e => e.EpisodeNumber == w.WatchedEpisode)
            })
            .Where(x => x.Episode != null)
            .OrderByDescending(x => x.Episode!.LastWatched)
            .Take(take)
            .Select(w => w.Id)
            .ToListAsync();

        return await _catalogService.GetOrCreateMediaCardsFromIds(results);
    }
}

using System.Net.NetworkInformation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Yugen.Core.Configs;
using Yugen.Core.Data;
using Yugen.Data;
using Yugen.Domain.Data.Downloads;
using Yugen.Domain.Data.Media;
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
    private readonly CatalogService _catalogService;
    private readonly ILibraryProvider _libraryProvider;

    public LibraryService(YugenContext db, IOptions<ProviderConfig> options, CatalogService catalogService)
    {
        _db = db;

        _catalogService = catalogService;
        _libraryProvider = new SonarrLibraryProvider(options.Value.sonarr_Url!, options.Value.sonarr_ApiKey!);
    }

    public async Task ResyncLibrary(UserModel user)
    {

    }

    public async Task<MediaCard[]> GetCurrentlyWatching(UserModel user)
    {
        return null;
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
            media.AniDbId = info.aniDbId;

            await _db.downloadedMedia.AddAsync(media);
        }
        else if (!force)
            return media;

        media.LastChecked = DateTime.UtcNow;
        Model_Link? link = await _db.links.FirstOrDefaultAsync(l => l.anidbid == media.AniDbId);

        if (link == null)
        {
            await _db.SaveChangesAsync();
            return null;
        }

        _db.sonarrEpisodes.RemoveRange(media.downloadedEpisodes);
        media.downloadedEpisodes.Clear();

        Model_DownloadedEpisode[]? episodes = await _libraryProvider.GetDownloadedEpisodes(aniListId, link!);

        media.downloadedEpisodes = episodes ?? [];
        await _db.SaveChangesAsync();

        return media;
    }
}

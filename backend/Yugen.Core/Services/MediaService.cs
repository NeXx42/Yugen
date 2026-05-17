using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Yugen.Core.Configs;
using Yugen.Data;
using Yugen.Domain.Data.Users;
using Yugen.Domain.Models;
using Yugen.Domain.Models.History;
using Yugen.Domain.Models.Library;
using Yugen.Providers;
using Yugen.Providers.Jellyfin;

namespace Yugen.Core.Services;

public class MediaService
{
    private readonly YugenContext _db;
    private readonly IMediaProvider _mediaProvider;

    public MediaService(YugenContext db, SettingsCache settings)
    {
        _db = db;
        _mediaProvider = new JellyfinMediaService(settings.Get(ConfigKeys.Jellyfin_Url), settings.Get(ConfigKeys.Jellyfin_ApiKey));
    }

    public async Task<string> Play()
    {
        return await _mediaProvider.Play();
    }

    public async Task<bool> LinkSonarrToJellyfin(Model_DownloadedMedia media)
    {
        string?[]? jellyfinIds = await _mediaProvider.MapPathToJellyfinId(media.downloadedEpisodes);

        if (jellyfinIds == null)
            return false;

        for (int i = 0; i < jellyfinIds.Length; i++)
            media.downloadedEpisodes.ElementAt(i).JellyfinId = jellyfinIds[i];

        return true;
    }

    public async Task SyncWatchHistoryWithJellyfin(UserSession usr, int AniListId, bool force = false)
    {
        Model_DownloadedMedia? downloadedMedia = await _db.downloadedMedia.Include(m => m.downloadedEpisodes).FirstOrDefaultAsync(m => m.MediaId == AniListId);

        if (downloadedMedia == null)
            return; // its not downloaded, so nothing to sync with

        Model_WatchHistory? history = await _db.watchHistory.Include(w => w.WatchedEpisodes).FirstOrDefaultAsync(w => w.MediaId == AniListId);

        if (history == null)
        {
            history ??= new Model_WatchHistory()
            {
                MediaId = AniListId,
                UpdatedTime = DateTime.UtcNow,
            };

            await _db.AddAsync(history);
        }
        else if (!force && history.UpdatedTime.HasValue && history.UpdatedTime.Value.AddMinutes(-30) <= DateTime.UtcNow)
            return;

        history.UpdatedTime = DateTime.UtcNow;

        int? latestWatchedEpisode = null;
        DateTime latestWatchedTime = DateTime.MinValue;

        Model_WatchedEpisode[] episodeData = await _mediaProvider.UpdateWatchHistory(usr.JellyfinId, downloadedMedia.downloadedEpisodes);

        foreach (Model_WatchedEpisode ep in episodeData)
        {
            if (ep.WatchPercentage > 0 && ep.LastWatched > latestWatchedTime)
                latestWatchedEpisode = ep.EpisodeNumber;

            Model_WatchedEpisode? existing = history.WatchedEpisodes.FirstOrDefault(w => w.EpisodeNumber == ep.EpisodeNumber);

            if (existing == null)
            {
                history.WatchedEpisodes.Add(ep);
            }
            else
            {
                existing.LastWatched = ep.LastWatched;
                existing.WatchPercentage = ep.WatchPercentage;
            }
        }

        history.WatchedEpisode = latestWatchedEpisode;
        await _db.SaveChangesAsync();
    }
}

using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Yugen.Core.Configs;
using Yugen.Data;
using Yugen.Domain.Data.Media;
using Yugen.Domain.Data.Users;
using Yugen.Domain.Models;
using Yugen.Domain.Models.History;
using Yugen.Domain.Models.Library;
using Yugen.Domain.Models.Media;
using Yugen.Providers;
using Yugen.Providers.Jellyfin;

namespace Yugen.Core.Services;

public class MediaService
{
    private readonly YugenContext _db;
    private readonly CacheService _cache;

    private readonly IMediaProvider _mediaProvider;

    public MediaService(YugenContext db, SettingsCache settings, CacheService cache)
    {
        _db = db;
        _cache = cache;

        _mediaProvider = new JellyfinMediaService(settings.Get(ConfigKeys.Jellyfin_Url), settings.Get(ConfigKeys.Jellyfin_ApiKey));
    }

    public async Task<PlaybackInfo> GetPlaybackInfo(UserSession usr, int? anilistId, int? episodeNumber, string jellyfinId)
    {
        PlaybackInfo info = await _mediaProvider.GetPlaybackInfo(jellyfinId);

        if (episodeNumber.HasValue && anilistId.HasValue)
        {
            Model_WatchedEpisode? episodeWatchData = await _db.watchedEpisodes.FirstOrDefaultAsync(e => e.MediaId == anilistId && e.EpisodeNumber == episodeNumber);

            if (episodeWatchData != null)
                info.historicalTicks = episodeWatchData.PlaybackPositionTicks;
        }

        return info;
    }

    public async Task<HttpRequestMessage> GetPlaybackRequest(UserSession usr, string jellyfinId, string mediaId)
    {
        string url = await _mediaProvider.GetPlaybackUrl(jellyfinId, mediaId);
        HttpRequestMessage http = new HttpRequestMessage(HttpMethod.Get, url);

        return http;
    }

    public async Task<HttpRequestMessage> GetSubtitleRequest(UserSession usr, string jellyfinId, string mediaId, int subtitleId)
    {
        string url = await _mediaProvider.GetSubtitleUrl(jellyfinId, mediaId, subtitleId);
        HttpRequestMessage http = new HttpRequestMessage(HttpMethod.Get, url);

        return http;
    }




    public async Task<string?[]?> GetJellyfinIdsForEpisodes(ICollection<Model_DownloadedEpisode> episodes) => await _mediaProvider.MapPathToJellyfinId(episodes);

    public async Task UpdateEpisodeWatchTime(UserSession usr, int AniListId, int epNumber, float percentage, long ticks)
    {
        Model_WatchHistory? history = await _db.watchHistory.Include(e => e.WatchedEpisodes).FirstOrDefaultAsync(e => e.MediaId == AniListId);

        if (history == null)
        {
            // should be empty
            _db.RemoveRange(_db.watchedEpisodes.Where(e => e.MediaId == AniListId));

            history = new Model_WatchHistory()
            {
                MediaId = AniListId,
                UpdatedTime = DateTime.UtcNow,
                WatchedEpisode = epNumber,

                WatchedEpisodes = [
                    new Model_WatchedEpisode(){
                        MediaId = AniListId,
                        EpisodeNumber = epNumber,

                        PlaybackPositionTicks = ticks,
                        WatchPercentage = percentage,
                        LastWatched = DateTime.UtcNow,
                    }
                ]
            };

            await _db.SaveChangesAsync();
            return;
        }

        history.UpdatedTime = DateTime.UtcNow;
        history.WatchedEpisode = epNumber;

        Model_WatchedEpisode? ep = history.WatchedEpisodes.FirstOrDefault(e => e.EpisodeNumber == epNumber);

        if (ep == null)
        {
            history.WatchedEpisodes.Add(new Model_WatchedEpisode()
            {
                MediaId = AniListId,
                EpisodeNumber = epNumber,

                PlaybackPositionTicks = ticks,
                WatchPercentage = percentage,
                LastWatched = DateTime.UtcNow,
            });
        }
        else
        {
            ep.PlaybackPositionTicks = ticks;
            ep.WatchPercentage = percentage;
        }

        await _db.SaveChangesAsync();
        _cache.Remove(CatalogService.GetCardCacheId(AniListId));
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


        int? latestWatchedEpisode = null;
        DateTime latestWatchedTime = DateTime.MinValue;

        Model_WatchedEpisode[] episodeData = await _mediaProvider.UpdateWatchHistory(usr.JellyfinId, downloadedMedia.downloadedEpisodes);

        foreach (Model_WatchedEpisode ep in episodeData)
        {
            if (ep.LastWatched.HasValue && ep.WatchPercentage > 0 && ep.LastWatched > latestWatchedTime)
            {
                latestWatchedTime = ep.LastWatched.Value;
                latestWatchedEpisode = ep.EpisodeNumber;
            }

            Model_WatchedEpisode? existing = history.WatchedEpisodes.FirstOrDefault(w => w.EpisodeNumber == ep.EpisodeNumber);

            if (existing == null)
            {
                history.WatchedEpisodes.Add(ep);
            }
            else
            {
                existing.PlaybackPositionTicks ??= ep.PlaybackPositionTicks;
                existing.LastWatched ??= ep.LastWatched;
                existing.WatchPercentage ??= ep.WatchPercentage;
            }
        }

        history.UpdatedTime ??= DateTime.UtcNow;
        history.WatchedEpisode ??= latestWatchedEpisode;
        await _db.SaveChangesAsync();

        _cache.Remove(CatalogService.GetCardCacheId(AniListId));
    }

    public async Task UploadSubtitle(string jellyfinId, string language, IFormFile subtitle)
    {
        string data;

        using (var stream = subtitle.OpenReadStream())
        using (var reader = new StreamReader(stream))
        {
            data = await reader.ReadToEndAsync();
        }

        if (string.IsNullOrEmpty(data))
            throw new Exception("Failed to read data");

        string base64Data = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(data));
        await _mediaProvider.UploadSubtitle(jellyfinId, language, Path.GetExtension(subtitle.FileName).TrimStart('.'), base64Data);
    }

    public async Task DeleteSubtitle(string jellyfinId, int id)
    {
        await _mediaProvider.DeleteSubtitle(jellyfinId, id);
    }
}

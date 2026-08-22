using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Yugen.Core.Configs;
using Yugen.Data;
using Yugen.Domain.Data.Downloads;
using Yugen.Domain.Data.Media;
using Yugen.Domain.Data.Users;
using Yugen.Domain.Interfaces;
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

    public MediaService(YugenContext db, SettingsCache settings, CacheService cache, ILogging logger)
    {
        _db = db;
        _cache = cache;

        _mediaProvider = new JellyfinMediaService(settings.Get(ConfigKeys.Jellyfin_Url), settings.Get(ConfigKeys.Jellyfin_ApiKey), logger);
    }

    public async Task<PlaybackInfo> GetPlaybackInfo(UserSession usr, int? anilistId, int? episodeNumber, string jellyfinId)
    {
        PlaybackInfo info = await _mediaProvider.GetPlaybackInfo(jellyfinId);

        if (episodeNumber.HasValue && anilistId.HasValue)
        {
            Model_WatchedEpisode? episodeWatchData = await _db.watchHistory
                .Where(w => w.UserId == usr.User.Id && w.MediaId == anilistId)
                .Include(w => w.WatchedEpisodes)
                .SelectMany(w => w.WatchedEpisodes)
                .FirstOrDefaultAsync(e => e.EpisodeNumber == episodeNumber);

            if (episodeWatchData != null)
                info.historicalTicks = episodeWatchData.PlaybackPositionTicks;
        }

        return info;
    }

    public async Task<string> ProxyUrl(string relative, bool includeApiKey) => await _mediaProvider.ProxyUrl(relative, includeApiKey);

    public async Task<HttpRequestMessage> GetPlaybackRequest(UserSession usr, string jellyfinId, int source, bool hls, long? bitrate = null, string? videoCodecs = "", string? audioCodecs = "", int? audioIndex = null)
    {
        string url = await _mediaProvider.GetPlaybackUrl(jellyfinId, source, hls, bitrate, videoCodecs, audioCodecs, audioIndex);
        HttpRequestMessage http = new HttpRequestMessage(HttpMethod.Get, url);

        return http;
    }

    public async Task<HttpRequestMessage> GetSubtitleRequest(UserSession usr, string jellyfinId, string mediaId, int subtitleId)
    {
        string url = await _mediaProvider.GetSubtitleUrl(jellyfinId, mediaId, subtitleId);
        HttpRequestMessage http = new HttpRequestMessage(HttpMethod.Get, url);

        return http;
    }




    public async Task<string?[]?> GetJellyfinIdsForEpisodes(UserSession usr, ICollection<Model_DownloadedEpisode> episodes) => await _mediaProvider.MapPathToJellyfinId(usr, episodes);

    public async Task UpdateEpisodeWatchTime(UserSession usr, int AniListId, int epNumber, float percentage, long ticks)
    {
        Model_WatchHistory? history = await _db.watchHistory.Include(e => e.WatchedEpisodes).FirstOrDefaultAsync(e => e.MediaId == AniListId);

        if (history == null)
        {
            await _db.AddAsync(new Model_WatchHistory()
            {
                MediaId = AniListId,
                UpdatedTime = DateTime.UtcNow,
                LastWatchedEpisodeNumber = epNumber,
                UserId = usr.User.Id,

                WatchedEpisodes = [
                    new Model_WatchedEpisode(){
                        EpisodeNumber = epNumber,

                        PlaybackPositionTicks = ticks,
                        WatchPercentage = percentage,
                    }
                ]
            });

            await _db.SaveChangesAsync();
            _cache.Remove(CatalogService.GetCardCacheId(AniListId));

            return;
        }

        history.UpdatedTime = DateTime.UtcNow;
        history.LastWatchedEpisodeNumber = epNumber;

        Model_WatchedEpisode? ep = history.WatchedEpisodes.FirstOrDefault(e => e.EpisodeNumber == epNumber);

        if (ep == null)
        {
            history.WatchedEpisodes.Add(new Model_WatchedEpisode()
            {
                HistoryId = history.Id,
                EpisodeNumber = epNumber,

                PlaybackPositionTicks = ticks,
                WatchPercentage = percentage,
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

    public async Task<DownloadedEpisodeSubtitles[]> GetSubtitles(string[] jellyfinIds)
    {
        return await _mediaProvider.GetSubtitles(jellyfinIds);
    }
}

using Yugen.Domain.Data.Media;
using Yugen.Domain.Models.History;
using Yugen.Domain.Models.Library;
using Yugen.Providers.Helpers;

namespace Yugen.Providers.Jellyfin;

public class JellyfinMediaService : IMediaProvider
{
    private readonly RestfulHelper _http;

    private readonly string _url;
    private readonly string _apiKey;

    public JellyfinMediaService(string url, string apiKey)
    {
        _url = url;
        _apiKey = apiKey;

        _http = new RestfulHelper(url, new Dictionary<string, string>()
        {
            { "X-Emby-Token", apiKey}
        });
    }

    public async Task<PlaybackInfo> GetPlaybackInfo(string jellyfinId)
    {
        JellyfinResponse_MediaInfo playbackInfo = (await _http.SendRequest<JellyfinResponse_MediaInfo>($"Items/{jellyfinId}/PlaybackInfo", HttpMethod.Get))!;

        return new PlaybackInfo()
        {
            jellyfinId = jellyfinId,

            sources = playbackInfo.MediaSources?.Select(m => new PlaybackInfo.Source()
            {
                id = m.id!,
                subs = m.MediaStreams?.Where(m => m.Type?.Equals("Subtitle") ?? false).Select(s => new PlaybackInfo.Source.Subtitles()
                {
                    title = s.DisplayTitle ?? s.Title,
                    language = s.Language!,
                    isExternal = s.IsExternal ?? false,
                    uri = $"api/media/{jellyfinId}/{m.id}/{s.Index}/Subtitle",
                    id = s.Index,

                }).ToArray() ?? []

            }).ToArray() ?? []
        };
    }

    public async Task<string> GetPlaybackUrl(string jellyfinId, string mediaId) => $"{_url}/Videos/{jellyfinId}/stream.mkv?static=true&api_key={_apiKey}&mediaSourceId={mediaId}";
    public async Task<string> GetSubtitleUrl(string jellyfinId, string mediaId, int subtitleId) => $"{_url}/Videos/{jellyfinId}/{mediaId}/Subtitles/{subtitleId}/Stream.vtt?api_key={_apiKey}";

    public async Task<string?[]?> MapPathToJellyfinId(ICollection<Model_DownloadedEpisode> episodes)
    {
        JellyfinResponse_Page<Jellyfin_Response_Item>? items = await _http.SendRequest<JellyfinResponse_Page<Jellyfin_Response_Item>>("Items?Recursive=true&IncludeItemTypes=Movie,Episode&Fields=Id,Path", HttpMethod.Get);

        if (items == null)
            return null;

        string?[] results = new string[episodes.Count];

        for (int i = 0; i < results.Length; i++)
        {
            if (string.IsNullOrEmpty(episodes.ElementAt(i).filePath))
            {
                results[i] = null;
                continue;
            }

            ReadOnlySpan<char> normalizedPath = RemoveFirstSegment(episodes.ElementAt(i).filePath!);
            int pathLength = normalizedPath.Length;

            foreach (Jellyfin_Response_Item jellyfinItem in items.Items)
            {
                if (string.IsNullOrEmpty(jellyfinItem.path) || jellyfinItem.path.Length <= pathLength)
                    continue;

                if (jellyfinItem.path.AsSpan(jellyfinItem.path.Length - pathLength, pathLength).SequenceEqual(normalizedPath))
                {
                    results[i] = jellyfinItem.id;
                    break;
                }
            }
        }

        return results;

        // jellyfin used a different start then sonarr
        ReadOnlySpan<char> RemoveFirstSegment(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            ReadOnlySpan<char> span = path.AsSpan();

            int i = 0;

            if (span.Length > 0 && span[0] == '/')
                i++;

            while (i < span.Length && span[i] != '/')
                i++;

            if (i >= span.Length)
                return "/";

            return span.Slice(i);
        }
    }

    public async Task<Model_WatchedEpisode[]> UpdateWatchHistory(string userId, ICollection<Model_DownloadedEpisode> episodes)
    {
        Dictionary<string, int> jellyfinMapping = new Dictionary<string, int>();

        for (int i = 0; i < episodes.Count; i++)
        {
            if (string.IsNullOrEmpty(episodes.ElementAt(i).JellyfinId))
                continue;

            jellyfinMapping.Add(episodes.ElementAt(i).JellyfinId!, i);
        }

        string query = $"?Ids={string.Join("&Ids=", jellyfinMapping.Keys)}";
        JellyfinResponse_Page<Jellyfin_Response_History>? items = await _http.SendRequest<JellyfinResponse_Page<Jellyfin_Response_History>>(Path.Combine("Users", userId, $"Items{query}&Fields=RunTimeTicks"), HttpMethod.Get);

        if (items == null)
            return [];

        List<Model_WatchedEpisode> res = new List<Model_WatchedEpisode>();

        foreach (Jellyfin_Response_History history in items.Items)
        {
            if (jellyfinMapping.TryGetValue(history.id, out int index))
            {
                if (history.userData == null || history.runTimeTicks == null)
                    continue;

                res.Add(new Model_WatchedEpisode()
                {
                    MediaId = episodes.ElementAt(index).MediaId,
                    EpisodeNumber = episodes.ElementAt(index).EpisodeNumber,
                    LastWatched = history.userData.LastPlayedDate,
                    WatchPercentage = history.userData.played ? 1f : Math.Clamp(history.userData.playBackPositionTicks / history.runTimeTicks.Value, 0, 1),
                    PlaybackPositionTicks = history.userData.playBackPositionTicks
                });
            }
        }

        return res.ToArray();
    }

    public async Task UploadSubtitle(string jellyfinId, string language, string format, string data)
    {
        await _http.SendRequest($"Videos/{jellyfinId}/Subtitles", HttpMethod.Post, new
        {
            Language = language,
            Format = format,
            Data = data,
            IsForced = false,
            IsHearingImpaired = false
        });
    }


    public async Task DeleteSubtitle(string jellyfinId, int id)
    {
        await _http.SendRequest($"Videos/{jellyfinId}/Subtitles/{id}", HttpMethod.Delete);
    }
}

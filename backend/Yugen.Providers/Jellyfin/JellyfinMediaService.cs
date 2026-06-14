using System.Net.Http.Headers;
using Yugen.Domain.Data.Downloads;
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

    private async Task<JellyfinResponse_MediaInfo> GetPlaybackInfoInternal(string jellyfinId)
    {
        return (await _http.SendRequest<JellyfinResponse_MediaInfo>($"Items/{jellyfinId}/PlaybackInfo", HttpMethod.Get))!;
    }

    public async Task<PlaybackInfo> GetPlaybackInfo(string jellyfinId)
    {
        List<PlaybackInfo.Segment> segments = new List<PlaybackInfo.Segment>();
        List<double> chapters = new List<double>();

        try
        {
            JellyfinResponse_Chapters? itemChapters = await _http.SendRequest<JellyfinResponse_Chapters>($"Items?ids={jellyfinId}&fields=chapters", HttpMethod.Get);

            long? introStart = null;
            long? introEnd = null;

            long? endingStart = null;
            long? endingEnd = null;

            var item = itemChapters?.items?.FirstOrDefault();
            double runTimeTicks = item?.runTimeTicks ?? 1;

            if (item?.chapters != null)
            {

                foreach (var chapter in item.chapters)
                {
                    double percentagePos = (chapter.startPositionTicks / runTimeTicks) * 100;

                    if (percentagePos >= 1 && percentagePos <= 99) // no need to worry about start / end "chapters"
                        chapters.Add(percentagePos);

                    switch (chapter.Name)
                    {
                        case "OP":
                        case "Prolog":
                        case "Opening": introStart ??= chapter.startPositionTicks; break;

                        case "ED":
                        case "Ending": endingStart = chapter.startPositionTicks; break;

                        default:
                            if (introStart.HasValue && !introEnd.HasValue)
                            {
                                introEnd = chapter.startPositionTicks;
                                break;
                            }

                            if (endingStart.HasValue && !endingEnd.HasValue)
                            {
                                endingEnd = chapter.startPositionTicks;
                                break;
                            }
                            break;
                    }
                }

                endingEnd ??= item.runTimeTicks;

                if (introStart.HasValue && introEnd.HasValue)
                    segments.Add(new PlaybackInfo.Segment(introStart.Value, introEnd.Value, runTimeTicks));

                if (endingStart.HasValue && endingEnd.HasValue)
                    segments.Add(new PlaybackInfo.Segment(endingStart.Value, endingEnd.Value, runTimeTicks));
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to get segments - {e.Message}");
        }


        JellyfinResponse_MediaInfo playbackInfo = await GetPlaybackInfoInternal(jellyfinId);

        return new PlaybackInfo()
        {
            jellyfinId = jellyfinId,
            segments = segments.ToArray(),
            chapters = chapters.ToArray(),

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

                }).ToArray() ?? [],

                audio = m.MediaStreams?.Where(a => a.Type?.Equals("Audio") ?? false).Select(a => new PlaybackInfo.Source.Audio()
                {
                    id = a.Index,
                    title = a.DisplayTitle ?? a.Title,
                    isDefault = a.IsDefault ?? false

                }).ToArray() ?? []

            }).ToArray() ?? []
        };
    }

    public Task<string> ProxyUrl(string relative, bool includeApiKey = false) => Task.FromResult($"{_url}/{relative}{(includeApiKey ? $"&api_key={_apiKey}" : "")}");

    public async Task<string> GetPlaybackUrl(string jellyfinId, int source, bool hls, long? maxBitrate, string? videoCodecs, string? audioCodecs, int? audioIndex)
    {
        JellyfinResponse_MediaInfo info = await GetPlaybackInfoInternal(jellyfinId);
        JellyfinResponse_MediaInfo.MediaSource sourceObj = info.MediaSources![source];

        Dictionary<string, string> vidParams = new Dictionary<string, string>();
        AddVidParam("MediaSourceId", sourceObj.id);
        AddVidParam("PlaySessionId", info.playSessionId);
        AddVidParam("AudioStreamIndex", audioIndex?.ToString());

        if (!hls) return await ProxyUrl($"Videos/{jellyfinId}/stream.mkv?static=true&{FormatVidParams()}", true);

        AddVidParam("SegmentContainer", "mp4");
        AddVidParam("AudioCodec", audioCodecs);
        AddVidParam("videoCodec", videoCodecs);
        AddVidParam("Tag", Guid.NewGuid().ToString().Replace("-", string.Empty));
        AddVidParam("VideoBitrate", maxBitrate?.ToString());

        //AddVidParam("AudioCodec", "aac,opus,flac");
        //AddVidParam("videoCodec", "av1,h264,vp9");

        AddVidParam("TranscodingMaxAudioChannels", "2");
        AddVidParam("RequireAvc", "false");
        AddVidParam("EnableAudioVbrEncoding", "true");
        AddVidParam("h264-level", "51");
        AddVidParam("h264-videobitdepth", "10");
        AddVidParam("h264-profile", "high,main,baseline,constrainedbaseline");
        AddVidParam("av1-profile", "main");
        AddVidParam("av1-rangetype", "SDR");
        AddVidParam("av1-level", "19");
        AddVidParam("vp9-rangetype", "SDR");
        AddVidParam("h264-rangetype", "SDR");
        AddVidParam("h264-deinterlace", "SDR");

        return await ProxyUrl($"Videos/{jellyfinId}/master.m3u8?{FormatVidParams()}", true);

        void AddVidParam(string key, string? value, bool ignoreEmpty = true)
        {
            if (string.IsNullOrEmpty(value) && ignoreEmpty)
                return;

            vidParams[key] = value ?? "";
        }

        string FormatVidParams()
        {
            return string.Join("&", vidParams.Select(v => $"{v.Key}={v.Value}"));
        }
    }

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

    public async Task<DownloadedEpisodeSubtitles[]> GetSubtitles(ICollection<string> jellyfinIds)
    {
        if (jellyfinIds.Count == 0)
            return [];

        Dictionary<string, Task<JellyfinResponse_MediaInfo>> playbackInfoTasks = new Dictionary<string, Task<JellyfinResponse_MediaInfo>>();

        foreach (string str in jellyfinIds)
            playbackInfoTasks.Add(str, GetPlaybackInfoInternal(str));

        await Task.WhenAll(playbackInfoTasks.Values);

        List<DownloadedEpisodeSubtitles> results = new List<DownloadedEpisodeSubtitles>();

        Parallel.ForEach(playbackInfoTasks, (KeyValuePair<string, Task<JellyfinResponse_MediaInfo>> info) =>
        {
            // not sure when there are multiple sources / havent encountered it yet
            JellyfinResponse_MediaInfo.MediaSource.MediaStream[] subs = info.Value.Result.MediaSources?.FirstOrDefault()?.MediaStreams?.Where(s => s.Type?.Equals("Subtitle") ?? false).ToArray() ?? [];

            results.AddRange(subs.Select(s => new DownloadedEpisodeSubtitles()
            {
                jellyfinEpisodeId = info.Key,
                subtitleId = s.Index,

                title = s.DisplayTitle,
                languageCode = s.Language
            }));
        });

        return results.ToArray();
    }
}

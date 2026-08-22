using System.Net.Http.Headers;
using Yugen.Domain.Data.Downloads;
using Yugen.Domain.Data.Media;
using Yugen.Domain.Data.Users;
using Yugen.Domain.Interfaces;
using Yugen.Domain.Models.History;
using Yugen.Domain.Models.Library;
using Yugen.Providers.Helpers;

namespace Yugen.Providers.Jellyfin;

public class JellyfinMediaService : IMediaProvider
{
    private readonly RestfulHelper _http;

    private readonly string _url;
    private readonly string _apiKey;

    public JellyfinMediaService(string? url, string? apiKey, ILogging logger)
    {
        if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(apiKey))
        {
            logger.LogError(new Exception("Failed to start jellyfin service"));
            _http = null!;

            _url = string.Empty;
            _apiKey = string.Empty;

            return;
        }

        _url = url;
        _apiKey = apiKey;

        _http = new RestfulHelper(url, logger, new Dictionary<string, string>()
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

    public async Task<string?[]?> MapPathToJellyfinId(UserSession usr, ICollection<Model_DownloadedEpisode> episodes)
    {
        Dictionary<string, List<Jellyfin_Response_Item>> byFilename = new(StringComparer.OrdinalIgnoreCase);
        int offset = 0;

        while (true)
        {
            const int take = 500;

            string url = $"Items?Recursive=true&UserId={usr.JellyfinId}&IncludeItemTypes=Movie,Episode&Fields=Id,Path&StartIndex={offset}&Limit={take}";
            JellyfinResponse_Page<Jellyfin_Response_Item>? items = await _http.SendRequest<JellyfinResponse_Page<Jellyfin_Response_Item>>(url, HttpMethod.Get);

            if (items == null || items.Items.Length == 0)
                break;

            foreach (Jellyfin_Response_Item item in items.Items)
            {
                if (string.IsNullOrEmpty(item.path))
                    continue;

                string normalized = NormalizePath(item.path);
                string filename = normalized.Substring(normalized.LastIndexOf('/') + 1);

                if (!byFilename.TryGetValue(filename, out var list))
                    byFilename[filename] = list = new();

                list.Add(item);
            }

            offset += items.Items.Length; // advance by what was actually returned, not the requested page size
            if (offset >= items.TotalRecordCount)
                break;
        }

        string?[] results = new string?[episodes.Count];

        for (int i = 0; i < results.Length; i++)
        {
            string? filePath = episodes.ElementAt(i).filePath;
            if (string.IsNullOrEmpty(filePath))
            {
                results[i] = null;
                continue;
            }

            string normalizedEpisodePath = NormalizePath(filePath);

            string[] episodeSegments = normalizedEpisodePath.Split('/');
            string filename = episodeSegments[^1];

            if (!byFilename.TryGetValue(filename, out List<Jellyfin_Response_Item>? candidates) || candidates.Count == 0)
                continue;

            if (candidates.Count == 1)
            {
                results[i] = candidates[0].id;
                continue;
            }

            results[i] = FindClosestMatchingPath(candidates, episodeSegments).id;
        }

        return results;

        string NormalizePath(string path) => path.Replace('\\', '/').TrimEnd('/');

        Jellyfin_Response_Item FindClosestMatchingPath(List<Jellyfin_Response_Item> candidates, string[] episodeSegments)
        {
            List<(int pos, string[] parts)> candidatePartsLookup = Enumerable.Range(0, candidates.Count)
                .Select(c => (c, candidates[c].path!.Split("/").ToArray()))
                .ToList();

            int? bestLastOption = 0;

            for (int segmentPos = 2; segmentPos <= episodeSegments.Length; segmentPos++)
            {
                if (candidatePartsLookup.Count <= 1)
                    break;

                string comparisonSegment = episodeSegments[episodeSegments.Length - segmentPos];

                for (int i = candidatePartsLookup.Count - 1; i >= 0; i--)
                {
                    int candidateSegmentPos = candidatePartsLookup[i].parts.Length - segmentPos;

                    if (candidateSegmentPos < 0 || !candidatePartsLookup[i].parts[candidateSegmentPos].Equals(comparisonSegment, StringComparison.OrdinalIgnoreCase))
                    {
                        candidatePartsLookup.RemoveAt(i);
                        continue;
                    }

                    bestLastOption = candidatePartsLookup[i].pos;
                }
            }

            return candidates[bestLastOption.Value];
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

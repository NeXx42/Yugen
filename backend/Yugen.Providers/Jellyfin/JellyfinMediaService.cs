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

    public async Task<string?[]?> MapPathToJellyfinId(ICollection<Model_DownloadedEpisode> episodes)
    {
        JellyfinResponse_Page<Jellyfin_Response_Item>? items = await _http.SendRequest<JellyfinResponse_Page<Jellyfin_Response_Item>>("Items?Recursive=true&IncludeItemTypes=Movie,Episode&Fields=Id,Path");

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

    public async Task<string> Play()
    {
        string itemId = "016d249d8cdaa92c5e8234414bc842cf";
        return Path.Combine(_url, "Videos", itemId, $"stream?static=true&api_key={_apiKey}");

        //016d249d8cdaa92c5e8234414bc842cf
        return await _http.SendRequest<string>($"Videos/{itemId}/stream");
    }
}

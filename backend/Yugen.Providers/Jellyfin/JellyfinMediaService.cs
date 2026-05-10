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

    public async Task<string> Play()
    {
        string itemId = "016d249d8cdaa92c5e8234414bc842cf";
        return Path.Combine(_url, "Videos", itemId, $"stream?static=true&api_key={_apiKey}");

        //016d249d8cdaa92c5e8234414bc842cf
        return await _http.SendRequest<string>($"Videos/{itemId}/stream");
    }
}

using Yugen.Domain.Data.Media;
using Yugen.Providers.Helpers;

namespace Yugen.Providers.Jellyfin;

public class JellyfinLibraryService : ILibraryProvider
{
    private readonly RestfulHelper _http;

    public JellyfinLibraryService(string url, string apiKey)
    {
        _http = new RestfulHelper(url, new Dictionary<string, string>()
        {
            { "X-Emby-Token", apiKey}
        });
    }

    public async Task<ExternalMedia[]> GetExternalMedia(string jellyfinUserId)
    {
        const string filters = "?Recursive=true&IncludeItemTypes=Movie,Series&Fields=ProviderIds";
        JellyfinResponse_Page<JellyfinResponse_Media>? res = await _http.SendRequest<JellyfinResponse_Page<JellyfinResponse_Media>>($"Users/{jellyfinUserId}/Items{filters}");

        if (res == null)
            return [];

        return res.Items.Select(x => new ExternalMedia()
        {
            id = Guid.Parse(x.id),
            title = x.name,

            aniDb = x.providerIds!.AniList

        }).ToArray();
    }
}

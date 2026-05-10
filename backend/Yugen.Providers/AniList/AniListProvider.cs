using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Yugen.Core.Data;

namespace Yugen.Providers.AniList;

public class AniListProvider : IMetaDataProvider
{
    private readonly string _url;
    private readonly HttpClient _http;

    public AniListProvider()
    {
        _url = "https://graphql.anilist.co";
        _http = new HttpClient();
    }

    public async Task<MediaCard[]> SearchMedia(string textFilter)
    {
        string query = @"
        query ($search: String!) {
            Page {
                media(search: $search, type: ANIME) {
                id
                title {
                    romaji
                    english
                    native
                }
                }
            }
        }";

        AniListResponse_Search? res = await SendRequest<AniListResponse_Search>(query, new { search = textFilter });
        return [];
    }

    public async Task<T?> SendRequest<T>(string query, object variables)
    {
        var json = JsonSerializer.Serialize(new { query, variables });
        var response = await _http.PostAsync(_url, new StringContent(json, Encoding.UTF8, "application/json"));

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }
}

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Yugen.Domain.Data.Users;

namespace Yugen.Providers.Jellyfin;

public class JellyfinUserService : IUserProvider
{
    private readonly string _url;
    private readonly string _apiKey;

    private readonly HttpClient _http;

    public JellyfinUserService(string url, string apiKey)
    {
        _url = url;
        _apiKey = apiKey;

        _http = new HttpClient();
    }

    public async Task<ExternalUser[]> GetAllUsers()
    {
        return await SendRequest<ExternalUser[]>("Users");
    }

    public async Task<(object providerSession, ExternalUser externalUserId)?> LoginUser(string username, string password)
    {
        JellyfinResponse_Session? session = await SendRequest<JellyfinResponse_Session>("Users/AuthenticateByName", JsonSerializer.Serialize(new
        {
            Username = username,
            Pw = password
        }));

        if (session == null)
            return null;

        return (
            session,
            new ExternalUser()
            {
                Name = session.User.Name,
                ExternalId = session.User.Id,
            }
        );
    }

    private async Task<T?> SendRequest<T>(string uri, string? body = null)
    {
        HttpRequestMessage req = new HttpRequestMessage(string.IsNullOrEmpty(body) ? HttpMethod.Get : HttpMethod.Post, $"{_url}{uri}");
        req.Headers.Add("X-Emby-Token", _apiKey);

        if (!string.IsNullOrEmpty(body))
        {
            req.Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
        }

        var res = await _http.SendAsync(req);

        if (!res.IsSuccessStatusCode)
        {
            throw new Exception("Invalid request - " + res.ReasonPhrase);
        }

        return await res.Content.ReadFromJsonAsync<T>();
    }
}

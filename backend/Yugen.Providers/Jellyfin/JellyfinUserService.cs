using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Yugen.Domain.Data.Users;
using Yugen.Providers.Helpers;

namespace Yugen.Providers.Jellyfin;

public class JellyfinUserService : IUserProvider
{
    private readonly RestfulHelper _http;

    public JellyfinUserService(string url, string apiKey)
    {
        Console.WriteLine(url);
        Console.WriteLine(apiKey);

        _http = new RestfulHelper(url, new Dictionary<string, string>()
        {
            { "X-Emby-Token", apiKey}
        });
    }

    public async Task<ExternalUser[]> GetAllUsers()
    {
        return await _http.SendRequest<ExternalUser[]>("Users");
    }

    public async Task<string?> LoginUser(string username, string password)
    {
        JellyfinResponse_Session? session = await _http.SendRequest<JellyfinResponse_Session>("Users/AuthenticateByName", JsonSerializer.Serialize(new
        {
            Username = username,
            Pw = password
        }));

        if (session == null)
            return null;

        return session.User.Id;
    }
}

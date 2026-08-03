using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Yugen.Domain.Data.Users;
using Yugen.Domain.Interfaces;
using Yugen.Providers.Helpers;

namespace Yugen.Providers.Jellyfin;

public class JellyfinUserService : IUserProvider
{
    private readonly RestfulHelper _http;
    private readonly bool isSetup;

    public JellyfinUserService(string? url, string? apiKey, ILogging logger)
    {
        if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(apiKey))
        {
            _ = logger.LogError(new Exception("Failed to start jellyfin provider"));
            _http = null!;

            return;
        }

        isSetup = !string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(apiKey);

        _http = new RestfulHelper(url, logger, new Dictionary<string, string>()
        {
            { "X-Emby-Token", apiKey}
        });
    }

    public async Task<ExternalUser[]> GetAllUsers()
    {
        if (!isSetup)
            throw new Exception("Not setup");

        return (await _http.SendRequest<ExternalUser[]>("Users", HttpMethod.Get)) ?? [];
    }

    public async Task<string?> LoginUser(string username, string password)
    {
        if (!isSetup)
            throw new Exception("Not setup");

        JellyfinResponse_Session? session = await _http.SendRequest<JellyfinResponse_Session>("Users/AuthenticateByName", HttpMethod.Post, new
        {
            Username = username,
            Pw = password
        });

        if (session == null)
            return null;

        return session.User.Id;
    }
}

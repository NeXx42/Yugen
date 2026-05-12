using System.Reflection;
using Yugen.Providers.Helpers;

namespace Yugen.Providers.IdsMoe;

public class IdMoeLinkingProvider : ILinkingProvider
{
    private readonly RestfulHelper _http;
    private readonly PropertyInfo[] _idProps;

    public IdMoeLinkingProvider(string url, string apiKey)
    {
        _http = new RestfulHelper(url, new Dictionary<string, string>()
        {
            { "Authorization", $"Bearer {apiKey}" }
        });

        _idProps = typeof(IdMoe_Responses_Ids).GetProperties();
    }

    public async Task<Dictionary<string, string>?> GetMediaProviderIds(string aniListId)
    {
        if (string.IsNullOrEmpty(aniListId))
            throw new ArgumentException("Invalid anilist id");

        IdMoe_Responses_Ids? res = await _http.SendRequest<IdMoe_Responses_Ids>($"ids/{aniListId}?platform=anilist");

        if (res == null)
            return null;

        Dictionary<string, string> results = new Dictionary<string, string>();

        foreach (var prop in _idProps)
        {
            string? val = prop.GetValue(res)?.ToString();

            if (!string.IsNullOrEmpty(val))
                results.Add(prop.Name, val);
        }

        return results;
    }
}

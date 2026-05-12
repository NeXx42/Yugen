using System.Net.Http.Json;

namespace Yugen.Providers.Helpers;

public class RestfulHelper
{
    private readonly HttpClient _http;

    private readonly string _url;
    private readonly Dictionary<string, string>? _defaultHeaders;

    public RestfulHelper(string url, Dictionary<string, string>? defaultHeaders = null)
    {
        _http = new HttpClient();

        _url = url;
        _defaultHeaders = defaultHeaders;
    }

    public async Task<T?> SendRequest<T>(string uri, string? body = null)
    {
        HttpRequestMessage req = new HttpRequestMessage(string.IsNullOrEmpty(body) ? HttpMethod.Get : HttpMethod.Post, Path.Combine(_url, uri));

        if (_defaultHeaders != null)
            foreach (KeyValuePair<string, string> cookie in _defaultHeaders)
                req.Headers.Add(cookie.Key, cookie.Value);

        if (!string.IsNullOrEmpty(body))
        {
            req.Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
        }

        var res = await _http.SendAsync(req);

        if (!res.IsSuccessStatusCode)
        {
            throw new Exception("Invalid request - " + res.ReasonPhrase);
        }

        Console.Write(await res.Content.ReadAsStringAsync());
        return await res.Content.ReadFromJsonAsync<T>();
    }
}

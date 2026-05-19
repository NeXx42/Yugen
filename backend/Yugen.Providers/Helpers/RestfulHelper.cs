using System.Net.Http.Json;
using System.Text.Json;

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

    public async Task<T?> SendRequest<T>(string uri, object? body = null)
    {
        HttpRequestMessage req = new HttpRequestMessage(body == null ? HttpMethod.Get : HttpMethod.Post, Path.Combine(_url, uri));

        if (_defaultHeaders != null)
            foreach (KeyValuePair<string, string> cookie in _defaultHeaders)
                req.Headers.Add(cookie.Key, cookie.Value);

        if (body != null)
        {
            string json = JsonSerializer.Serialize(body);
            req.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        }

        var httpRes = await _http.SendAsync(req);
        string response = await httpRes.Content.ReadAsStringAsync();

        if (!httpRes.IsSuccessStatusCode)
        {
            Console.Write(response);
            throw new Exception("Invalid request - " + httpRes.ReasonPhrase);
        }

        try
        {
            T? res = JsonSerializer.Deserialize<T>(response);
            return res;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(response);
            return default;
        }
    }
}

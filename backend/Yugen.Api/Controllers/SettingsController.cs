using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yugen.Api.Helpers;
using Yugen.Core.Services;
using Yugen.Domain.Data;
using Yugen.Domain.Data.Users;

namespace Yugen.Api.Controllers;

[ApiController]
[Route("api/Settings")]
public class SettingsController : ControllerBase
{
    private readonly SettingsService _settings;

    public SettingsController(SettingsService settings)
    {
        _settings = settings;
    }

    [HttpGet("Setup")]
    public async Task<bool> VerifyJellyfinSetup()
    {
        string? url = _settings.getCache.Get(ConfigKeys.Jellyfin_Url);
        string? key = _settings.getCache.Get(ConfigKeys.Jellyfin_Url);

        return !string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(key);
    }

    public class SetupRequest
    {
        public string? url { get; set; }
        public string? key { get; set; }
    }

    [HttpPost("Setup")]
    public async Task<IResult> SetupJellyfin([FromBody] SetupRequest req)
    {
        if (await VerifyJellyfinSetup())
            return Results.BadRequest("Already setup");

        try
        {
            using (HttpClient client = new HttpClient())
            {
                HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, Path.Combine(req.url ?? "", "System", "Endpoint"));
                msg.Headers.Add("X-Emby-Token", req.key);

                var res = await client.SendAsync(msg);

                res.EnsureSuccessStatusCode();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return Results.BadRequest("Invalid credentials");
        }

        await _settings.SetConfigValue(ConfigKeys.Jellyfin_Url, req.url);
        await _settings.SetConfigValue(ConfigKeys.Jellyfin_ApiKey, req.key);

        return Results.Ok();
    }

    public class ConfigRequest
    {
        public string? value { get; set; }
    }

    [HttpPost("{Key}")]
    [Authorize]
    public async Task<IResult> SaveConfigValue([FromBody] ConfigRequest req, string Key)
    {
        if (!Enum.TryParse(Key, out ConfigKeys key))
            return Results.NotFound();

        await _settings.SetConfigValue(key, req.value);
        return Results.Ok();
    }

    public class ConfigResponse
    {
        public required string key { get; set; }
        public string? value { get; set; }
    }

    [HttpGet()]
    [Authorize]
    public async Task<ConfigResponse[]> Get()
    {
        return (await _settings.GetAllCache()).Select(c => new ConfigResponse()
        {
            key = c.Key.ToString(),
            value = c.Value
        }).ToArray();
    }

    [HttpPost("Update")]
    [Authorize]
    public async Task<IActionResult> TriggerUpdate()
    {
        try
        {
            HttpContext.GetUserFromSession(out UserSession usr);
            await _settings.TriggerUpdate(usr);

            return Ok();
        }
        catch
        {
            return BadRequest();
        }
    }

    [HttpGet("Links")]
    [Authorize]
    public async Task<Links> GetLinks() => await _settings.GetLinks();
}
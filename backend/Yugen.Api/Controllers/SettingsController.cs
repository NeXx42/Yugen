using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yugen.Api.Helpers;
using Yugen.Core.Services;

namespace Yugen.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/Settings")]
public class SettingsController : ControllerBase
{
    private readonly SettingsService _settings;

    public SettingsController(SettingsService settings)
    {
        _settings = settings;
    }

    public class ConfigRequest
    {
        public string? value { get; set; }
    }

    [HttpPost("{Key}")]
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
    public async Task<ConfigResponse[]> Get()
    {
        return (await _settings.GetAllCache()).Select(c => new ConfigResponse()
        {
            key = c.Key.ToString(),
            value = c.Value
        }).ToArray();
    }
}
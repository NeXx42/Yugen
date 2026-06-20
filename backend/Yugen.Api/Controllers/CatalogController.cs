using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yugen.Api.Helpers;
using Yugen.Core.Data;
using Yugen.Core.Services;
using Yugen.Domain.Data;
using Yugen.Domain.Data.Media;
using Yugen.Domain.Data.Users;
using Yugen.Domain.Enums;

namespace Yugen.Api.Controllers;

[ApiController]
[Route("api/catalog")]
public class CatalogController : ControllerBase
{
    private readonly CatalogService _catalogService;

    public CatalogController(CatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [HttpGet("Upcoming")]
    public async Task<MediaCard[]> GetUpcoming([FromQuery] int? take)
    {
        return await _catalogService.Upcoming(take ?? 10);
    }

    [HttpGet("Trending")]
    public async Task<MediaInfo[]> GetTrending([FromQuery] int? take)
    {
        return await _catalogService.GetTrending(take ?? 10);
    }

    [HttpPost("Search")]
    public async Task<PageResponse<MediaCard>> Search([FromBody] MediaSearchQuery query) => await _catalogService.Search(query);

    [HttpGet("{id}")]
    public async Task<MediaInfo?> GetMediaInfo(int id)
    {
        HttpContext.TryGetUserFromSession(out UserSession? usr);
        return await _catalogService.GetMediaInfoForUser(usr, id);
    }

    [HttpPost("RedownloadLinking")]
    [Authorize]
    public async Task<bool> RedownloadLinks()
    {
        try
        {
            HttpContext.GetUserFromSession(out UserSession usr);
            await _catalogService.RedownloadLinks(usr);
            return true;
        }
        catch
        {
            return false;
        }
    }

    [HttpGet("SearchCriteria")]
    public async Task<SearchCriteria> GetSearchCriteria() => await _catalogService.GetSearchCriteria();

    [HttpPost("Cache/DatabaseClear")]
    [Authorize]
    public async Task ClearDatabaseCache() => await _catalogService.ClearDatabaseCache();

    [HttpPost("Cache/Clear")]
    [Authorize]
    public async Task ClearCache() => await _catalogService.ClearCache();
}

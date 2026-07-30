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
    private readonly LinkService _linkService;
    private readonly CatalogService _catalogService;

    public CatalogController(CatalogService catalogService, LinkService linkService)
    {
        _catalogService = catalogService;
        _linkService = linkService;
    }

    [HttpGet("Upcoming")]
    public async Task<IResult> GetUpcoming([FromQuery] int? take)
    {
        return await ExceptionWrapper.WrapException(() => _catalogService.Upcoming(take ?? 10));
    }

    [HttpGet("Trending")]
    public async Task<IResult> GetTrending([FromQuery] int? take)
    {
        return await ExceptionWrapper.WrapException(() => _catalogService.GetTrending(take ?? 10));
    }

    [HttpPost("Search")]
    public async Task<IResult> Search([FromBody] MediaSearchQuery query)
    {
        return await ExceptionWrapper.WrapException(() => _catalogService.Search(query));
    }

    [HttpGet("{id}")]
    public async Task<IResult> GetMediaInfo(int id)
    {
        HttpContext.TryGetUserFromSession(out UserSession? usr);
        return await ExceptionWrapper.WrapException(() => _catalogService.GetMediaInfoForUser(usr, id));
    }

    [HttpPost("RedownloadLinking")]
    [Authorize]
    public async Task<bool> RedownloadLinks()
    {
        try
        {
            HttpContext.GetUserFromSession(out UserSession usr);
            await _linkService.RedownloadLinks(usr);
            return true;
        }
        catch
        {
            return false;
        }
    }

    [HttpGet("SearchCriteria")]
    public async Task<IResult> GetSearchCriteria()
    {
        return await ExceptionWrapper.WrapException(_catalogService.GetSearchCriteria);
    }

    [HttpPost("Cache/DatabaseClear")]
    [Authorize]
    public async Task<IResult> ClearDatabaseCache()
    {
        return await ExceptionWrapper.WrapException(_catalogService.ClearDatabaseCache);
    }

    [HttpPost("Cache/Clear")]
    [Authorize]
    public async Task<IResult> ClearCache()
    {
        return await ExceptionWrapper.WrapException(_catalogService.ClearCache);
    }
}

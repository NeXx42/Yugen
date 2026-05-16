using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yugen.Core.Data;
using Yugen.Core.Services;
using Yugen.Domain.Data.Media;

namespace Yugen.Api.Controllers;

[ApiController]
[Authorize]
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

    public class Search_Query
    {
        public string? text { get; set; }
    }

    [HttpPost("Search")]
    public async Task<MediaCard[]> Search([FromBody] Search_Query query)
    {
        return await _catalogService.Search(query.text!);
    }

    [HttpGet("{id}")]
    public async Task<MediaInfo?> GetMediaInfo(int id)
    {
        return await _catalogService.GetMediaInfo(id);
    }

    [HttpPost("RedownloadLinking")]
    public async Task<bool> RedownloadLinks()
    {
        try
        {
            await _catalogService.RedownloadLinks();
            return true;
        }
        catch
        {
            return false;
        }
    }
}

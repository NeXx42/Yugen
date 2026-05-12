using Microsoft.AspNetCore.Mvc;
using Yugen.Core.Data;
using Yugen.Core.Services;
using Yugen.Domain.Data.Media;

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
    public async Task<MediaCard[]> GetUpcoming()
    {
        return await _catalogService.Upcoming();
    }

    [HttpGet("Search")]
    public async Task<MediaCard[]> Search([FromQuery] string query)
    {
        return await _catalogService.Search(query);
    }

    [HttpGet("{id}")]
    public async Task<MediaInfo?> GetMediaInfo(int id)
    {
        return await _catalogService.GetMediaInfo(id);
    }
}

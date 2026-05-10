using Microsoft.AspNetCore.Mvc;
using Yugen.Core.Data;
using Yugen.Core.Services;

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
    public IResult GetUpcoming()
    {
        return Results.Ok();
    }

    [HttpGet("Search")]
    public async Task<MediaCard[]> Search([FromQuery] string query)
    {
        return await _catalogService.Search(query);
    }
}

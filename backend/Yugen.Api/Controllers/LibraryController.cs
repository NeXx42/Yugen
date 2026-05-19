using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yugen.Api.Helpers;
using Yugen.Core.Data;
using Yugen.Core.Services;
using Yugen.Domain.Data;
using Yugen.Domain.Data.Downloads;
using Yugen.Domain.Data.History;
using Yugen.Domain.Data.Media;
using Yugen.Domain.Data.Users;
using Yugen.Domain.Models;

namespace Yugen.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/library")]
public class LibraryController : ControllerBase
{
    private readonly LibraryService _libraryService;

    public LibraryController(LibraryService libraryService)
    {
        _libraryService = libraryService;
    }

    [HttpGet("WatchHistory")]
    public async Task<PageResponse<MediaCard>> GetWatchHistory([FromQuery] int? page, [FromQuery] int? pageSize)
    {
        return await _libraryService.GetWatchHistory(page ?? 0, pageSize ?? 10);
    }

    [HttpPost("Sync/History")]
    public async Task SyncWatchHistory()
    {
        HttpContext.GetUserFromSession(out UserSession usr);
        await _libraryService.SyncWatchHistory(usr);
    }

    [HttpPost("Sync/Library")]
    public async Task<int?> SyncExternalLibraries()
    {
        HttpContext.GetUserFromSession(out UserSession usr);
        return await _libraryService.ResyncLibrary(usr);
    }

    public class SearchLibraryFilter
    {
        public int? page { get; set; }
        public int? pageSize { get; set; }
        public string? group { get; set; }
    }

    [HttpPost("Search")]
    public async Task<PageResponse<MediaCard>> SearchLibrary([FromBody] SearchLibraryFilter filter)
    {
        HttpContext.GetUserFromSession(out UserSession usr);
        return await _libraryService.SearchLibrary(usr, filter.page ?? 0, filter.pageSize ?? 10, filter.group ?? "all");
    }

    [HttpPost("Upload")]
    public async Task<IResult> UploadLibrary(IFormFile? file)
    {
        if ((file?.Length ?? 0) == 0)
            return Results.BadRequest();

        HttpContext.GetUserFromSession(out UserSession usr);
        await _libraryService.UploadLibrary(usr, file!);

        return Results.Ok();
    }

    [HttpPost("{mediaId}/UpdateBookmark")]
    public async Task<IResult> UpdateBookmark(int mediaId, [FromQuery] int id)
    {
        try
        {
            HttpContext.GetUserFromSession(out UserSession usr);
            await _libraryService.UpdateBookmark(usr, mediaId, id);
        }
        catch
        {
            return Results.InternalServerError();
        }

        return Results.Ok();
    }

    public class SeriesRequest
    {
        public required string rootPath { get; set; }
        public required int quality { get; set; }
    }

    [HttpPost("{mediaId}/Request")]
    public async Task<IResult> RequestSeries(int mediaId, [FromBody] SeriesRequest request)
    {
        HttpContext.GetUserFromSession(out UserSession usr);
        bool res = await _libraryService.RequestSeries(usr, mediaId, request.rootPath, request.quality);

        if (res)
            return Results.Ok();

        return Results.BadRequest();
    }

    [HttpGet("{id}/Episodes")]
    public async Task<EpisodeInfo[]> GetMediaEpisodes(int id, [FromQuery] bool refetch = false)
    {
        HttpContext.GetUserFromSession(out UserSession usr);
        return await _libraryService.GetMediaEpisodesForUser(usr, id, refetch);
    }
}

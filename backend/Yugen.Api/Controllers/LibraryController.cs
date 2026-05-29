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

    [HttpGet("{id}/Film")]
    public async Task<EpisodeInfo?> GetFilmEpisodeContainer(int id, [FromQuery] bool refetch = false)
    {
        HttpContext.GetUserFromSession(out UserSession usr);
        return await _libraryService.GetFilmEpisodeContainer(usr, id, refetch);
    }

    [HttpGet("{id}/Episodes")]
    public async Task<EpisodeInfo[]> GetMediaEpisodes(int id, [FromQuery] bool refetch = false)
    {
        HttpContext.GetUserFromSession(out UserSession usr);
        return await _libraryService.GetMediaEpisodesForUser(usr, id, refetch);
    }

    [HttpGet("WatchHistory")]
    public async Task<PageResponse<MediaCard>> GetWatchHistory([FromQuery] int? page, [FromQuery] int? pageSize)
    {
        return await _libraryService.GetWatchHistory(page ?? 0, pageSize ?? 10);
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

    [HttpPost("{mediaId}/Request")]
    public async Task<IResult> RequestSeries(int mediaId, [FromBody] DownloadRequest request)
    {
        HttpContext.GetUserFromSession(out UserSession usr);
        bool res = await _libraryService.RequestSeries(usr, mediaId, request);

        if (res)
            return Results.Ok();

        return Results.BadRequest();
    }

    [HttpGet("{mediaId}/Request")]
    public async Task<DownloadRequestInfo> GetSeriesRequestInfo(int mediaId)
    {
        HttpContext.GetUserFromSession(out UserSession usr);
        return await _libraryService.GetSeriesRequestInfo(usr, mediaId);
    }

    [HttpPost("{mediaId}/SyncDownloads")]
    public async Task SyncMediaDownloads(int mediaId, [FromQuery] bool force)
    {
        HttpContext.GetUserFromSession(out UserSession usr);
        await _libraryService.RecheckDownloads(usr, mediaId, force);
    }

    [HttpPost("{mediaId}/ResearchDownloads")]
    public async Task ResearchDownloads(int mediaId)
    {
        HttpContext.GetUserFromSession(out UserSession usr);
        await _libraryService.ResearchDownloads(usr, mediaId);
    }

    [HttpDelete("{mediaId}")]
    public async Task Delete(int mediaId)
    {
        HttpContext.GetUserFromSession(out UserSession usr);
        await _libraryService.DeleteMedia(usr, mediaId);
    }
}

using System.DirectoryServices.Protocols;
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
using Yugen.Domain.Enums;
using Yugen.Domain.Models;

namespace Yugen.Api.Controllers;

[ApiController]
[Route("api/library")]
public class LibraryController : ControllerBase
{
    private readonly LibraryService _libraryService;
    private readonly LinkService _linkingServer;

    public LibraryController(LibraryService libraryService, LinkService linkingService)
    {
        _libraryService = libraryService;
        _linkingServer = linkingService;
    }

    [HttpGet("{id}/Film")]
    public async Task<EpisodeInfo?> GetFilmEpisodeContainer(int id, [FromQuery] bool refetch = false)
    {
        HttpContext.TryGetUserFromSession(out UserSession? usr);
        return await _libraryService.GetFilmEpisodeContainer(usr, id, refetch);
    }

    [HttpGet("{id}/Episodes")]
    public async Task<EpisodeInfo[]> GetMediaEpisodes(int id, [FromQuery] bool refetch = false, [FromQuery] bool clearOld = false)
    {
        HttpContext.TryGetUserFromSession(out UserSession? usr);
        return await _libraryService.GetMediaEpisodesForUser(usr, id, refetch, clearOld);
    }

    [HttpGet("WatchHistory")]
    [Authorize]
    public async Task<PageResponse<MediaCard>> GetWatchHistory([FromQuery] int? page, [FromQuery] int? pageSize)
    {
        HttpContext.GetUserFromSession(out UserSession usr);

        return await _libraryService.GetWatchHistory(usr, new MediaSearchQuery
        {
            page = page,
            pageSize = pageSize
        });
    }

    [HttpPost("Sync/Library")]
    [Authorize]
    public async Task<int?> SyncExternalLibraries()
    {
        HttpContext.GetUserFromSession(out UserSession usr);
        return await _libraryService.ResyncLibrary(usr);
    }

    public class SearchLibraryFilter
    {
        public string? group { get; set; }
        public MediaSearchQuery? req { get; set; }
    }

    [HttpPost("Search")]
    [Authorize]
    public async Task<PageResponse<MediaCard>> SearchLibrary([FromBody] SearchLibraryFilter filter)
    {
        HttpContext.GetUserFromSession(out UserSession usr);
        return await _libraryService.SearchLibrary(usr, filter.req, filter.group ?? "all");
    }

    [HttpPost("Upload")]
    [Authorize]
    public async Task<IResult> UploadLibrary(IFormFile? file)
    {
        if ((file?.Length ?? 0) == 0)
            return Results.BadRequest();

        HttpContext.GetUserFromSession(out UserSession usr);
        await _libraryService.UploadLibrary(usr, file!);

        return Results.Ok();
    }

    [HttpPost("{mediaId}/UpdateBookmark")]
    [Authorize]
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
    [Authorize]
    public async Task<IResult> RequestSeries(int mediaId, [FromBody] DownloadRequest request)
    {
        HttpContext.GetUserFromSession(out UserSession usr);
        bool res = await _libraryService.RequestSeries(usr, mediaId, request);

        if (res)
            return Results.Ok();

        return Results.BadRequest();
    }

    [HttpGet("{mediaId}/Request")]
    [Authorize]
    public async Task<DownloadRequestInfo> GetSeriesRequestInfo(int mediaId)
    {
        HttpContext.GetUserFromSession(out UserSession usr);
        return await _libraryService.GetSeriesRequestInfo(usr, mediaId);
    }

    [HttpPost("{mediaId}/SyncDownloads")]
    [Authorize]
    public async Task SyncMediaDownloads(int mediaId, [FromQuery] bool force)
    {
        HttpContext.GetUserFromSession(out UserSession usr);
        await _libraryService.RecheckDownloads(usr, mediaId, force);
    }

    [HttpPost("{mediaId}/ResearchDownloads")]
    [Authorize]
    public async Task ResearchDownloads(int mediaId)
    {
        HttpContext.GetUserFromSession(out UserSession usr);
        await _libraryService.ResearchDownloads(usr, mediaId);
    }

    [HttpDelete("{mediaId}")]
    [Authorize]
    public async Task Delete(int mediaId)
    {
        HttpContext.GetUserFromSession(out UserSession usr);
        await _libraryService.DeleteMedia(usr, mediaId);
    }

    [HttpPost("{mediaId}/ClearHistory")]
    [Authorize]
    public async Task<IResult> ClearSeriesHistory(int mediaId)
    {
        try
        {
            HttpContext.GetUserFromSession(out UserSession usr);
            await _libraryService.ClearMediaHistory(usr, mediaId);

            return Results.Ok();
        }
        catch
        {
            return Results.BadRequest();
        }
    }

    public struct MediaLinkRequest
    {
        public int libraryProvider { get; set; }
        public int linkedId { get; set; }
        public int? linkedSeason { get; set; }
    }

    [HttpPost("{mediaId}/SaveLink")]
    public async Task<IResult> SaveManualLink(int mediaId, [FromBody] MediaLinkRequest req)
    {
        try
        {
            HttpContext.GetUserFromSession(out UserSession usr);
            await _linkingServer.SaveManualLink(usr, (LibraryProviderType)req.libraryProvider, mediaId, req.linkedId, req.linkedSeason);

            return Results.Ok();
        }
        catch
        {
            return Results.InternalServerError();
        }
    }
}

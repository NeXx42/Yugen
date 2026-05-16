using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yugen.Api.Helpers;
using Yugen.Core.Data;
using Yugen.Core.Services;
using Yugen.Domain.Data.Downloads;
using Yugen.Domain.Data.History;
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

    [HttpGet("currentlyWatching")]
    public async Task<MediaCard[]> GetCurrentlyWatching()
    {
        return await _libraryService.GetWatchHistory(10);
    }

    [HttpGet("{id}")]
    public async Task<DownloadedEpisode[]> GetDownloadedEpisodes(int id)
    {
        return await _libraryService.GetDownloadedEpisodes(id);
    }

    [HttpGet("{seriesId}/WatchHistory")]
    public async Task<WatchHistoryContainer?> GetEpisodeWatchHistory(int seriesId)
    {
        HttpContext.GetUserFromSession(out UserSession usr);
        return await _libraryService.GetEpisodeWatchHistory(usr, seriesId);
    }

    [HttpPost("Sync/History")]
    public async Task SyncWatchHistory()
    {
        HttpContext.GetUserFromSession(out UserSession usr);
        await _libraryService.SyncWatchHistory(usr);
    }

    [HttpPost("Sync/Library")]
    public async Task SyncExternalLibraries()
    {
        HttpContext.GetUserFromSession(out UserSession usr);
        await _libraryService.ResyncLibrary(usr);
    }

}

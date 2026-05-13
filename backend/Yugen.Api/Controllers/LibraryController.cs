using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yugen.Api.Helpers;
using Yugen.Core.Data;
using Yugen.Core.Services;
using Yugen.Domain.Data.Downloads;
using Yugen.Domain.Models;

namespace Yugen.Api.Controllers;

[ApiController]
[Route("api/library")]
public class LibraryController : ControllerBase
{
    private readonly LibraryService _libraryService;

    public LibraryController(LibraryService libraryService)
    {
        _libraryService = libraryService;
    }

    [HttpPost("sync")]
    [Authorize]
    public async Task SyncExternalLibraries()
    {
        HttpContext.GetUserFromSession(out UserModel usr);
        await _libraryService.ResyncLibrary(usr);
    }

    [HttpGet("currentlyWatching")]
    [Authorize]
    public async Task<MediaCard[]> GetCurrentlyWatching()
    {
        HttpContext.GetUserFromSession(out UserModel usr);
        return await _libraryService.GetCurrentlyWatching(usr);
    }

    [HttpGet("{id}")]
    public async Task<DownloadedEpisode[]> GetDownloadedEpisodes(int id)
    {
        return await _libraryService.GetDownloadedEpisodes(id);
    }
}

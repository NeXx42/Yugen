using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yugen.Api.Helpers;
using Yugen.Core.Services;
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
}

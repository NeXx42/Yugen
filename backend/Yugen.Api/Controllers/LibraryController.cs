using Microsoft.AspNetCore.Mvc;

namespace Yugen.Api.Controllers;

[ApiController]
[Route("api/library")]
public class LibraryController : ControllerBase
{
    [HttpPost("sync")]
    public void SyncExternalLibraries()
    {

    }
}

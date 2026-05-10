using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yugen.Core.Services;

namespace Yugen.Api.Controllers;

[ApiController]
[Route("api/media")]
public class MediaController : ControllerBase
{
    private readonly MediaService _mediaService;

    public MediaController(MediaService mediaService)
    {
        _mediaService = mediaService;
    }

    [HttpGet("play")]
    public async Task<IActionResult> Play()
    {
        string url = await _mediaService.Play();
        return Redirect("https://jellyfin.local/Videos/3a1340d44e2b8c59eb25226608786fb6/stream.mp4?&api_key=d5be01fa82e5452e923a2fcb19a02350");
    }
}
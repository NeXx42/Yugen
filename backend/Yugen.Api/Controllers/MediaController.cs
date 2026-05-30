using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Yugen.Api.Helpers;
using Yugen.Core.Services;
using Yugen.Domain.Data.Media;
using Yugen.Domain.Data.Users;

namespace Yugen.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/media")]
public class MediaController : ControllerBase
{
    private static readonly HttpClient _client = new HttpClient();
    private readonly MediaService _mediaService;

    public MediaController(MediaService mediaService)
    {
        _mediaService = mediaService;
    }

    [HttpGet("{jellyfinId}/PlaybackInfo")]
    public async Task<PlaybackInfo> PlaybackInfo(string jellyfinId, [FromQuery] int? anilistId, [FromQuery] int? episodeNumber)
    {
        HttpContext.GetUserFromSession(out var usr);
        return await _mediaService.GetPlaybackInfo(usr, anilistId, episodeNumber, jellyfinId);
    }

    [HttpGet("{jellyfinId}/{source}/stream.mkv")]
    public async Task Stream(string jellyfinId, int source)
    {
        HttpContext.GetUserFromSession(out var usr);
        HttpRequestMessage request = await _mediaService.GetPlaybackRequest(usr, jellyfinId, source, false);

        if (Request.Headers.TryGetValue("Range", out var rangeHeader) && System.Net.Http.Headers.RangeHeaderValue.TryParse(rangeHeader, out var range))
            request.Headers.Range = range;

        using (HttpResponseMessage response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
        {
            Response.StatusCode = (int)response.StatusCode;
            Response.ContentType = response.Content.Headers.ContentType?.ToString() ?? "video/x-matroska";
            Response.Headers["Content-Range"] = response.Content.Headers.ContentRange!.ToString();
            Response.Headers["Accept-Ranges"] = "bytes";

            if (response.Content.Headers.ContentLength.HasValue)
                Response.ContentLength = response.Content.Headers.ContentLength.Value;

            await using Stream upstream = await response.Content.ReadAsStreamAsync();
            await upstream.CopyToAsync(Response.Body, 81920);
        }
    }

    [HttpGet("{jellyfinId}/{source}/stream.m3u8")]
    public async Task StreamHLS(string jellyfinId, int source, [FromQuery] long? bitrate, [FromQuery] string videoCodecs, [FromQuery] string audioCodecs, [FromQuery] int? audioStreamIndex)
    {
        HttpContext.GetUserFromSession(out var usr);
        HttpRequestMessage request = await _mediaService.GetPlaybackRequest(usr, jellyfinId, source, true, bitrate, videoCodecs, audioCodecs, audioStreamIndex);

        await ProxyRequest(request);
    }

    [HttpGet("{jellyfinId}/{source}/main.m3u8")]
    public async Task StreamHLS_Main(string jellyfinId)
    {
        string url = await _mediaService.ProxyUrl($"Videos/{jellyfinId}/main.m3u8{HttpContext.Request.QueryString.Value}", false);
        await ProxyRequest(new HttpRequestMessage(HttpMethod.Get, url));
    }

    [HttpGet("{jellyfinId}/{source}/hls1/main/{segmentId}.{container}")]
    public async Task StreamHLS_Segment(string jellyfinId, string segmentId, string container)
    {
        string url = await _mediaService.ProxyUrl($"Videos/{jellyfinId}/hls1/main/{segmentId}.{container}{HttpContext.Request.QueryString.Value}", false);
        await ProxyRequest(new HttpRequestMessage(HttpMethod.Get, url));
    }

    private async Task ProxyRequest(HttpRequestMessage request)
    {
        using (HttpResponseMessage response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
        {
            Response.StatusCode = (int)response.StatusCode;
            Response.ContentType = response.Content.Headers.ContentType?.ToString() ?? "application/x-mpegURL";

            foreach (var header in response.Content.Headers)
                Response.Headers[header.Key] = new StringValues(header.Value.ToArray());

            await using Stream upstream = await response.Content.ReadAsStreamAsync();
            await upstream.CopyToAsync(Response.Body);
        }
    }



    [HttpGet("{jellyfinId}/{mediaId}/{subtitleId}/Subtitle")]
    public async Task GetSubtitle(string jellyfinId, string mediaId, int subtitleId)
    {
        HttpContext.GetUserFromSession(out var usr);
        HttpRequestMessage request = await _mediaService.GetSubtitleRequest(usr, jellyfinId, mediaId, subtitleId);

        using (HttpResponseMessage response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
        {
            Response.StatusCode = (int)response.StatusCode;
            Response.ContentType = response.Content.Headers.ContentType?.ToString();

            if (response.Content.Headers.ContentLength.HasValue)
                Response.ContentLength = response.Content.Headers.ContentLength.Value;

            await using Stream upstream = await response.Content.ReadAsStreamAsync();
            await upstream.CopyToAsync(Response.Body, 81920);
        }
    }

    public class EpisodeWatchTimeUpdate()
    {
        public float percentage { get; set; }
        public double runtimeSeconds { get; set; }
    }

    [HttpPost("{mediaId}/{episode}/UpdateTime")]
    public async Task UpdateEpisodeWatchTime(int mediaId, int episode, [FromBody] EpisodeWatchTimeUpdate data)
    {
        long ticks = new DateTimeOffset().AddSeconds(data.runtimeSeconds).Ticks;

        HttpContext.GetUserFromSession(out var usr);
        await _mediaService.UpdateEpisodeWatchTime(usr, mediaId, episode, data.percentage, ticks);
    }

    [HttpPost("{jellyfinId}/UploadSubtitle")]
    public async Task<IActionResult> UploadSubtitle(string jellyfinId, IFormFile? subtitle, [FromQuery] string language)
    {
        if (string.IsNullOrEmpty(language))
            return BadRequest("Invalid language");

        if (subtitle == null)
            return BadRequest("Invalid file");

        await _mediaService.UploadSubtitle(jellyfinId, language, subtitle);
        return Ok();
    }

    [HttpDelete("{jellyfinId}/{subtitleId}/Subtitle")]
    public async Task<IActionResult> DeleteSubtitle(string jellyfinId, int subtitleId)
    {
        await _mediaService.DeleteSubtitle(jellyfinId, subtitleId);
        return Ok();
    }
}
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SQLitePCL;
using Yugen.Api.Helpers;
using Yugen.Core.Services;
using Yugen.Domain.Data;
using Yugen.Domain.Data.Users;
using Yugen.Domain.Enums;

namespace Yugen.Api.Controllers;

[ApiController]
[Route("api/Notifications")]
public class NotificationController : ControllerBase
{
    private readonly NotificationService _notificationService;

    public NotificationController(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpPost("Sonarr")]
    public async Task SonarrWebhook()
    {
        using StreamReader reader = new StreamReader(HttpContext.Request.Body);
        string json = await reader.ReadToEndAsync();

        await _notificationService.ConsumeWebhook(json, LibraryProviderType.Sonarr);
    }

    [HttpGet("Count")]
    [Authorize]
    public async Task<int> GetNotificationCount()
    {
        HttpContext.GetUserFromSession(out UserSession usr);
        return await _notificationService.GetNotificationCount(usr);
    }

    [HttpGet]
    [Authorize]
    public async Task<Notification[]> GetNotifications()
    {
        HttpContext.GetUserFromSession(out UserSession usr);
        return await _notificationService.GetNotifications(usr);
    }

    [HttpPost("{id}/Read")]
    [Authorize]
    public async Task ReadNotification(int id)
    {
        HttpContext.GetUserFromSession(out UserSession usr);
        await _notificationService.ReadNotification(usr, id);
    }

    [HttpPost("{id}/Remove")]
    [Authorize]
    public async Task RemoveNotification(int id)
    {
        HttpContext.GetUserFromSession(out UserSession usr);
        await _notificationService.RemoveNotification(usr, id);
    }

    [HttpPost("Clear")]
    public async Task ClearReadNotifications()
    {
        HttpContext.GetUserFromSession(out UserSession usr);
        await _notificationService.ClearReadNotifications(usr);
    }

    [HttpPost("ReadAll")]
    public async Task ReadAllNotifications([FromQuery] string[] sources)
    {
        HttpContext.GetUserFromSession(out UserSession usr);
        await _notificationService.MarkAllAsRead(usr, sources);
    }
}

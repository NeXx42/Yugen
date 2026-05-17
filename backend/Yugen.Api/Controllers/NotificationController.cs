using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Yugen.Core.Services;
using Yugen.Domain.Enums;

namespace Yugen.Api.Controllers;

[ApiController]
[Route("Notifications")]
public class NotificationController : ControllerBase
{
    public class WebhookMessage
    {
        public Series? series { get; set; }

        public string? eventType { get; set; }
        public string? instanceName { get; set; }

        public class Series
        {
            public int? id { get; set; }
            public string? title { get; set; }
            public string? titleSlug { get; set; }
            public string? path { get; set; }
            public int? tvdbId { get; set; }
            public int? tvMazeId { get; set; }
            public int? tmdbId { get; set; }
            public string? imdbId { get; set; }
            public string? type { get; set; }
            public int? year { get; set; }
        }
    }

    private readonly NotificationService _notificationService;

    public NotificationController(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpPost]
    public async Task Webhook()
    {
        using StreamReader reader = new StreamReader(HttpContext.Request.Body);
        string json = await reader.ReadToEndAsync();

        Console.WriteLine(json);
        WebhookMessage? msg = JsonSerializer.Deserialize<WebhookMessage>(json);

        if (string.IsNullOrEmpty(msg?.eventType) || !Enum.TryParse(msg.eventType, out SonarrWebhookEventType eventType))
        {
            Console.WriteLine($"Failed to parse - {msg?.eventType}");
            return;
        }

        await _notificationService.SaveNotification(eventType, msg?.series?.tvdbId, msg?.series?.tmdbId);
    }
}

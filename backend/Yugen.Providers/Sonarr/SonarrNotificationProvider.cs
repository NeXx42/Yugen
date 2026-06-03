using System.Text.Json;
using Yugen.Domain.Models;
using Yugen.Domain.Models.Linking;

namespace Yugen.Providers.Sonarr;

public class SonarrNotificationProvider : INotificationService
{
    public async Task<Model_Notification[]> Consume(string json, Func<int?, Task<Model_Link?>> lookupExternalId, Func<int, Task> refreshDownloads)
    {
        SonarrNotification_WebhookMessage? msg = JsonSerializer.Deserialize<SonarrNotification_WebhookMessage>(json);

        if (string.IsNullOrEmpty(msg?.eventType) || !Enum.TryParse(msg.eventType, out SonarrWebhookEventType eventType))
        {
            Console.WriteLine($"Failed to parse - {msg?.eventType}");
            return [];
        }

        Model_Link? mediaId = await lookupExternalId(msg.series?.tvdbId);

        if (mediaId != null)
        {
            switch (eventType)
            {
                case SonarrWebhookEventType.Download:
                case SonarrWebhookEventType.SeriesDelete:
                case SonarrWebhookEventType.EpisodeFileDelete:
                case SonarrWebhookEventType.SeriesAdd:
                    await refreshDownloads(mediaId.anilist_id!.Value);
                    break;
            }

            switch (eventType)
            {
                case SonarrWebhookEventType.SeriesDelete:
                case SonarrWebhookEventType.Download:
                case SonarrWebhookEventType.SeriesAdd:
                case SonarrWebhookEventType.EpisodeFileDelete:
                case SonarrWebhookEventType.ManualInteractionRequired:
                    return [new Model_Notification()
                    {
                        Date = DateTime.UtcNow,
                        EventName = eventType.ToString(),
                        UserId = Guid.Empty,
                        MediaId = mediaId.anilist_id!.Value,
                        Message = eventType.ToString(),
                        Source = "Sonarr"
                    }];
            }
        }

        return [];
    }
}

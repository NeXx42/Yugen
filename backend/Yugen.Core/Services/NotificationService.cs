using Microsoft.EntityFrameworkCore;
using Yugen.Core.Data;
using Yugen.Data;
using Yugen.Domain.Data;
using Yugen.Domain.Data.Users;
using Yugen.Domain.Enums;
using Yugen.Domain.Models;
using Yugen.Domain.Models.Linking;
using Yugen.Providers;
using Yugen.Providers.Sonarr;

namespace Yugen.Core.Services;

public class NotificationService
{
    private readonly YugenContext _db;
    private readonly CatalogService _catalog;
    private readonly LibraryService _library;

    public NotificationService(YugenContext db, CatalogService catalog, LibraryService library)
    {
        _db = db;
        _catalog = catalog;
        _library = library;
    }

    private INotificationService GetProvider(LibraryProviderType provider)
    {
        switch (provider)
        {
            case LibraryProviderType.Sonarr:
                return new SonarrNotificationProvider();
        }

        return null;
    }

    public async Task ConsumeWebhook(string json, LibraryProviderType provider)
    {
        Model_Notification[] notifications = await GetProvider(provider).Consume(json, FetchLinking, RefreshDownloads);

        if (notifications.Length > 0)
        {
            Guid[] users = await _db.user.Select(u => u.Id).ToArrayAsync();
            List<Model_Notification> notificationsToAdd = new List<Model_Notification>();

            foreach (Model_Notification template in notifications)
            {
                foreach (Guid usr in users)
                {
                    notificationsToAdd.Add(new Model_Notification()
                    {
                        Date = DateTime.UtcNow,
                        EventName = template.EventName,
                        UserId = usr,
                        MediaId = template.MediaId,
                        MediaEpisode = template.MediaEpisode,
                        Message = template.Message,
                        Source = template.Source,
                        HasInteracted = false
                    });
                }
            }

            await _db.notifications.AddRangeAsync(notificationsToAdd);
            await _db.SaveChangesAsync();
        }

        async Task<Model_Link?> FetchLinking(int? tvid)
        {
            if (!tvid.HasValue)
                return null;

            Model_Link? link = await _db.links.FirstOrDefaultAsync(l => l.tvdb_id == tvid);
            return link;
        }

        async Task RefreshDownloads(int mediaId) => await _library.RecheckDownloads(UserSession.Master, mediaId, true);
    }

    public async Task<Notification[]> GetNotifications(UserSession usr)
    {
        Model_Notification[] notifis = await _db.notifications.Where(n => n.UserId == usr.User.Id).Take(99).ToArrayAsync();

        List<int> requiredMediaInfo = notifis.Where(n => n.MediaId.HasValue).Select(n => n.MediaId!.Value).Distinct().ToList();
        MediaCard[] cards = await _catalog.GetOrCreateMediaCardsFromIds(requiredMediaInfo);

        Notification[] results = new Notification[notifis.Length];

        for (int i = 0; i < notifis.Length; i++)
        {
            Model_Notification n = notifis[i];
            MediaCard? media = cards.FirstOrDefault(c => c.aniListId == n.MediaId);

            results[i] = new Notification()
            {
                id = n.Id,
                time = new DateTimeOffset(n.Date).ToUnixTimeMilliseconds(),

                eventName = n.EventName,
                reason = n.Message,
                source = n.Source,

                media = media,

                hasBeenSeen = n.HasInteracted,

                url = $"{n.MediaId}{(n.MediaEpisode.HasValue ? $"?episode={n.MediaEpisode}" : "")}"
            };
        }

        return results.ToArray();
    }

    public async Task<int> GetNotificationCount(UserSession usr) => await _db.notifications.Where(n => n.UserId == usr.User.Id && n.HasInteracted != true).CountAsync();

    public async Task ReadNotification(UserSession usr, int id)
    {
        Model_Notification? notif = await _db.notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == usr.User.Id);

        if (notif == null)
            return;

        notif.HasInteracted = true;
        await _db.SaveChangesAsync();
    }

    public async Task RemoveNotification(UserSession usr, int id)
    {
        _db.Remove(_db.notifications.First(n => n.Id == id && n.UserId == usr.User.Id));
        await _db.SaveChangesAsync();
    }

    public async Task ClearReadNotifications(UserSession usr)
    {
        _db.RemoveRange(_db.notifications.Where(n => n.UserId == usr.User.Id && n.HasInteracted));
        await _db.SaveChangesAsync();
    }

    public async Task MarkAllAsRead(UserSession usr, string[] sources)
    {
        Model_Notification[] notifications = await _db.notifications.Where(n => n.UserId == usr.User.Id && !n.HasInteracted && (sources.Length == 0 || sources.Contains(n.Source))).ToArrayAsync();

        foreach (Model_Notification notification in notifications)
            notification.HasInteracted = true;

        await _db.SaveChangesAsync();
    }
}

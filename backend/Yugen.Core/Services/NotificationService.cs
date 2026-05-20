using Microsoft.EntityFrameworkCore;
using Yugen.Core.Data;
using Yugen.Data;
using Yugen.Domain.Data;
using Yugen.Domain.Data.Users;
using Yugen.Domain.Enums;
using Yugen.Domain.Models;
using Yugen.Domain.Models.Linking;

namespace Yugen.Core.Services;

public class NotificationService
{
    private readonly YugenContext _db;
    private readonly CatalogService _catalog;

    public NotificationService(YugenContext db, CatalogService catalog)
    {
        _db = db;
        _catalog = catalog;
    }

    public async Task SaveNotification(SonarrWebhookEventType type, int? tvdbId, int? tmdbId)
    {
        Model_Link? link = await _db.links.FirstOrDefaultAsync(l => l.tvdb_id == tvdbId);

        if (link?.anilist_id == null)
            return;

        Guid[] users = await _db.user.Select(u => u.Id).ToArrayAsync();

        List<Model_Notification> toAdd = new List<Model_Notification>();

        foreach (Guid usr in users)
        {
            toAdd.Add(new Model_Notification()
            {
                MediaId = link.anilist_id.Value,
                EventType = type,
                UserId = usr,
                Date = DateTime.UtcNow,
            });
        }

        await _db.AddRangeAsync(toAdd);
        await _db.SaveChangesAsync();
    }

    public async Task<Notification[]> GetNotifications(UserSession usr)
    {
        Model_Notification[] notifis = await _db.notifications.Where(n => n.UserId == usr.User.Id).Take(99).ToArrayAsync();
        MediaCard[] cards = await _catalog.GetOrCreateMediaCardsFromIds(notifis.Select(n => n.MediaId).Distinct().ToList());

        Notification[] results = new Notification[notifis.Length];

        for (int i = 0; i < notifis.Length; i++)
        {
            Model_Notification n = notifis[i];
            MediaCard? media = cards.FirstOrDefault(c => c.aniListId == n.MediaId);

            results[i] = new Notification()
            {
                id = n.Id,
                time = new DateTimeOffset(n.Date).ToUnixTimeMilliseconds(),

                eventName = n.EventType.ToString(),
                reason = n.Message,

                title = media?.Title,
                icon = media?.cardImg,
                bannerIcon = media?.banner,

                hasBeenSeen = n.HasInteracted,

                url = $"{n.MediaId}"
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

    public async Task ClearReadNotifications(UserSession usr)
    {
        _db.RemoveRange(_db.notifications.Where(n => n.UserId == usr.User.Id && n.HasInteracted));
        await _db.SaveChangesAsync();
    }
}

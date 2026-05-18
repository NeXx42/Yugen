using Microsoft.EntityFrameworkCore;
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

    public NotificationService(YugenContext db)
    {
        _db = db;
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
        var notifis = await (
            from n in _db.notifications
            join m in _db.media
                on n.MediaId equals m.Id into mediaJoin
            from m in mediaJoin.DefaultIfEmpty()
            select new
            {
                notification = n,
                media = m
            }
        ).ToArrayAsync();

        return notifis.Select(n => new Notification()
        {
            id = n.notification.Id,
            time = n.notification.Date.Ticks,

            eventName = n.notification.EventType.ToString(),
            title = n.media.Title,
            reason = n.notification.Message,
            icon = n.media.CardImageLarge,

            hasBeenSeen = n.notification.HasInteracted,

            url = $"{n.media.Id}"
        }).ToArray();
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

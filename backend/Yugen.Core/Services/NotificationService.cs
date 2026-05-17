using Microsoft.EntityFrameworkCore;
using Yugen.Data;
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
}

using Microsoft.Extensions.DependencyInjection;
using Yugen.Core.Services;

namespace Yugen.YugenBackgroundService.Jobs;

public class EpisodeNotificationJob : IScheduledJob
{
    public bool immediateStart => true;

    public async Task ExecuteAsync(IServiceScopeFactory scopeFactory, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();

        CatalogService catalogFactory = scope.ServiceProvider.GetRequiredService<CatalogService>();
        await catalogFactory.CheckForOutOfDateEpisodes();
    }

    public async Task WaitForNextTickAsync(CancellationToken token)
    {
        DateTime now = DateTime.Now;
        DateTime nextRun = now.Minute < 5
            ? new DateTime(now.Year, now.Month, now.Day, now.Hour, 5, 0)
            : now.Minute < 35
                ? new DateTime(now.Year, now.Month, now.Day, now.Hour, 35, 0)
                : new DateTime(now.Year, now.Month, now.Day, now.Hour, 5, 0).AddHours(1);

        await Task.Delay(nextRun - now, token);
    }
}

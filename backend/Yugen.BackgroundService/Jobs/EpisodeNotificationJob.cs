using Microsoft.Extensions.DependencyInjection;
using Yugen.Core.Services;

namespace Yugen.YugenBackgroundService.Jobs;

public class EpisodeNotificationJob : IScheduledJob
{
    public bool immediateStart => true;
    public TimeSpan GetInterval() => new TimeSpan(0, 30, 0);

    public async Task ExecuteAsync(IServiceScopeFactory scopeFactory, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();

        CatalogService catalogFactory = scope.ServiceProvider.GetRequiredService<CatalogService>();
        await catalogFactory.CheckForOutOfDateEpisodes();
    }
}

using Microsoft.Extensions.DependencyInjection;
using Yugen.Core.Services;

namespace Yugen.YugenBackgroundService.Jobs;

public class CacheClearJob : IScheduledJob
{
    public bool immediateStart => false;

    public async Task ExecuteAsync(IServiceScopeFactory factory, CancellationToken cancellationToken)
    {
        using var scope = factory.CreateScope();

        CatalogService catalogFactory = scope.ServiceProvider.GetRequiredService<CatalogService>();
        await catalogFactory.ClearCache();
    }

    public async Task WaitForNextTickAsync(CancellationToken token)
    {
        DateTime now = DateTime.Now;
        DateTime nextRun = now.Date.AddHours(2);

        if (now >= nextRun)
            nextRun = nextRun.AddDays(1);

        await Task.Delay(nextRun - now, token);
    }
}

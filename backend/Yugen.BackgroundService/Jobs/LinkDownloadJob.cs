using Microsoft.Extensions.DependencyInjection;
using Yugen.Core.Services;
using Yugen.YugenBackgroundService;

namespace Yugen.YugenBackgroundService.Jobs;

public class LinkDownloadJob : IScheduledJob
{
    public bool immediateStart => true;
    public TimeSpan GetInterval() => new TimeSpan(12, 0, 0);

    public async Task ExecuteAsync(IServiceScopeFactory factory, CancellationToken cancellationToken)
    {
        using var scope = factory.CreateScope();

        LinkService linkFactory = scope.ServiceProvider.GetRequiredService<LinkService>();
        await linkFactory.RedownloadLinks();
    }
}

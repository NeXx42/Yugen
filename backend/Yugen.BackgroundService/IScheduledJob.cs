using Microsoft.Extensions.DependencyInjection;

namespace Yugen.YugenBackgroundService;

public interface IScheduledJob
{
    public bool immediateStart { get; }

    public Task WaitForNextTickAsync(CancellationToken token);
    public Task ExecuteAsync(IServiceScopeFactory factory, CancellationToken cancellationToken);
}

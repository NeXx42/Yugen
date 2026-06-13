using Microsoft.Extensions.DependencyInjection;

namespace Yugen.YugenBackgroundService;

public interface IScheduledJob
{
    public bool immediateStart { get; }
    public TimeSpan GetInterval();

    public Task ExecuteAsync(IServiceScopeFactory factory, CancellationToken cancellationToken);
}

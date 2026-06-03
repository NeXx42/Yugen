using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Yugen.Core.Services;

public class SettingsInit : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public SettingsInit(IServiceScopeFactory scopeFactory, SettingsCache cache)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using (var scope = _scopeFactory.CreateAsyncScope())
        {
            SettingsService service = scope.ServiceProvider.GetRequiredService<SettingsService>();
            await service.OnLoad();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

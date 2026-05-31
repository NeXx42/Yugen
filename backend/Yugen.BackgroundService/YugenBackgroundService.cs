using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Yugen.Core.Services;
using Yugen.Data;
using Yugen.Domain.Models.Media;

namespace Yugen.YugenBackgroundService;

public class YugenBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public YugenBackgroundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(30));
        await Run();


        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await Run();
        }
    }

    private async Task Run()
    {
        using var scope = _scopeFactory.CreateScope();

        CatalogService catalogFactory = scope.ServiceProvider.GetRequiredService<CatalogService>();
        await catalogFactory.CheckForOutOfDateEpisodes();
    }
}

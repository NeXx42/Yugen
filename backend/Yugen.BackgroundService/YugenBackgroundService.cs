using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Yugen.Core.Services;
using Yugen.Data;
using Yugen.Domain.Models;
using Yugen.Domain.Models.Media;

namespace Yugen.YugenBackgroundService;

public class YugenBackgroundService : BackgroundService
{
    private readonly IEnumerable<IScheduledJob> _jobs;
    private readonly IServiceScopeFactory _scopeFactory;

    public YugenBackgroundService(IServiceScopeFactory scopeFactory, IEnumerable<IScheduledJob> jobs)
    {
        _jobs = jobs;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tasks = _jobs.Select(job => RunJobLoop(job, stoppingToken));
        await Task.WhenAll(tasks);
    }

    private async Task RunJobLoop(IScheduledJob job, CancellationToken cancellationToken)
    {
        if (job.immediateStart)
            await RunJob(job, cancellationToken);

        using var timer = new PeriodicTimer(job.GetInterval());

        while (await timer.WaitForNextTickAsync(cancellationToken))
            await RunJob(job, cancellationToken);
    }

    private async Task RunJob(IScheduledJob job, CancellationToken cancellationToken)
    {
        try
        {
            await job.ExecuteAsync(_scopeFactory, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in service\n{ex.Message}");

            try
            {
                using var scope = _scopeFactory.CreateScope();

                YugenContext db = scope.ServiceProvider.GetRequiredService<YugenContext>();
                await db.AddAsync(new Model_Exception()
                {
                    Message = ex.Message,
                    Trace = ex.StackTrace
                });

                await db.SaveChangesAsync();
            }
            catch (Exception e) { Console.WriteLine($"Failed to save exception to db\n{e.Message}"); }
        }
    }
}

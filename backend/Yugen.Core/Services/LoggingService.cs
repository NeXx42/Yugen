using Yugen.Data;
using Yugen.Domain.Interfaces;

namespace Yugen.Core.Services;

public class LoggingService : ILogging
{
    private readonly YugenContext _database;

    public LoggingService(YugenContext context)
    {
        _database = context;
    }

    public async Task LogError(Exception e)
    {
        await _database.exceptions.AddAsync(new Domain.Models.Model_Exception()
        {
            Message = e.Message,
            Trace = e.StackTrace,
            Time = DateTime.UtcNow
        });
        await _database.SaveChangesAsync();
    }
}

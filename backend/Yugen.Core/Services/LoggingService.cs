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

    public void LogError(Exception e)
    {
        _database.exceptions.Add(new Domain.Models.Model_Exception()
        {
            Message = e.Message,
            Trace = e.StackTrace,
            Time = DateTime.UtcNow
        });
        _database.SaveChanges();
    }
}

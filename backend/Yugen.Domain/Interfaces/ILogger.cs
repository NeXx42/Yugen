namespace Yugen.Domain.Interfaces;

public interface ILogging
{
    public Task LogError(Exception e);
}

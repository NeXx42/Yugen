namespace Yugen.Providers;

public interface IMediaProvider
{
    public Task<string> Play();
}

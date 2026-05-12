namespace Yugen.Providers;

public interface ILinkingProvider
{
    public Task<Dictionary<string, string>?> GetMediaProviderIds(string aniListId);
}

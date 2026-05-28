using Yugen.Domain.Enums;
using Yugen.Domain.Models.Library;
using Yugen.Domain.Models.Linking;
using Yugen.Domain.Models.Media;
using Yugen.Providers;

namespace Yugen.Core.Factories;

public class LibraryFactory
{
    private readonly ILibraryProvider _sonarr;
    private readonly ILibraryProvider _radarr;

    public LibraryFactory(ILibraryProvider sonarr, ILibraryProvider radarr)
    {
        _sonarr = sonarr;
        _radarr = radarr;
    }

    public ILibraryProvider GetFactory() => _sonarr;

    public ILibraryProvider GetFactory(Model_Link link) => GetFactory(link.type);
    public ILibraryProvider GetFactory(Model_Media media) => GetFactory(media.MediaFormat);
    public ILibraryProvider GetFactory(Model_DownloadedMedia media)
    {
        switch (media.ProviderType)
        {
            case Domain.Enums.LibraryProviderType.Radarr:
                return _radarr;

            default:
                return _sonarr;
        }
    }

    public ILibraryProvider GetFactory(string? type)
    {
        if (type?.Equals("movie", StringComparison.CurrentCultureIgnoreCase) ?? false)
            return _radarr;

        return _sonarr;
    }

    public ILibraryProvider GetFactory(int type) => GetFactory((LibraryProviderType)type);
    public ILibraryProvider GetFactory(LibraryProviderType type)
    {
        switch (type)
        {
            case LibraryProviderType.Radarr:
                return _radarr;

            default:
                return _sonarr;
        }
    }
}

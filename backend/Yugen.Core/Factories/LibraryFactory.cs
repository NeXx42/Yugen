using Yugen.Core.Services;
using Yugen.Domain.Enums;
using Yugen.Domain.Interfaces;
using Yugen.Domain.Models.Library;
using Yugen.Domain.Models.Linking;
using Yugen.Domain.Models.Media;
using Yugen.Providers;
using Yugen.Providers.Radarr;
using Yugen.Providers.Sonarr;

namespace Yugen.Core.Factories;

public class LibraryFactory
{
    public static LibraryFactory Create(SettingsCache settings, ILogging logging)
    {
        return new LibraryFactory(
            new SonarrLibraryProvider(settings.Get(ConfigKeys.Sonarr_Url), settings.Get(ConfigKeys.Sonarr_ApiKey), logging),
            new RadarrLibraryProvider(settings.Get(ConfigKeys.Radarr_Url), settings.Get(ConfigKeys.Radarr_ApiKey), logging)
        );
    }

    private readonly ILibraryProvider _sonarr;
    private readonly ILibraryProvider _radarr;

    public LibraryFactory(ILibraryProvider sonarr, ILibraryProvider radarr)
    {
        _sonarr = sonarr;
        _radarr = radarr;
    }

    public ILibraryProvider[] GetFactories() => [_sonarr, _radarr];

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

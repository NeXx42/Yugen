using Yugen.Domain.Data.Downloads;
using Yugen.Domain.Data.Media;
using Yugen.Domain.Enums;
using Yugen.Domain.Models.Library;
using Yugen.Domain.Models.Linking;
using Yugen.Providers.Helpers;

namespace Yugen.Providers.Sonarr;

public class SonarrLibraryProvider : ILibraryProvider
{
    private readonly RestfulHelper _http;
    private readonly bool isSetup;

    bool ILibraryProvider.isSetup => isSetup;

    public SonarrLibraryProvider(string url, string apiKey)
    {
        isSetup = !string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(apiKey);

        _http = new RestfulHelper(Path.Combine(url, "api", "v3"), new Dictionary<string, string>()
        {
            { "X-Api-Key", apiKey }
        });
    }

    public async Task<Model_DownloadedMedia?> GetDownloadedEpisodes(int mediaId, Model_Link link)
    {
        if (link.tvdb_id == null || link.tvdb_season == null) return null;

        SonarrLibrary_Response_Series? series = (await _http.SendRequest<SonarrLibrary_Response_Series[]>($"series?tvdbId={link.tvdb_id}", HttpMethod.Get))?.FirstOrDefault();
        if (series == null) return null;

        SonarrLibrary_Response_Series.Seasons? matchedSeason = series.seasons?.FirstOrDefault(s => s.seasonNumber == link.tvdb_season);
        if (matchedSeason == null) return null;

        SonarrLibrary_Response_Episode[]? episodes = await _http.SendRequest<SonarrLibrary_Response_Episode[]>($"episode?seriesId={series.id}&seasonNumber={matchedSeason.seasonNumber}", HttpMethod.Get);
        if (episodes == null) return null;

        Model_DownloadedEpisode[] foundEpisodes = episodes.Select(e => new Model_DownloadedEpisode()
        {
            Id = e.id,
            fileId = e.episodeFileId == 0 ? null : e.episodeFileId,
            monitored = e.monitored,

            MediaId = mediaId,
            EpisodeNumber = e.episodeNumber,
        }).ToArray();

        if (foundEpisodes.Length == 0)
            return null;

        string fileIds = string.Join("&episodeFileIds=", foundEpisodes.Select(e => e.fileId).Where(x => x.HasValue));
        SonarrLibrary_Response_EpisodeFile[]? files = [];

        if (!string.IsNullOrEmpty(fileIds))
            files = (await _http.SendRequest<SonarrLibrary_Response_EpisodeFile[]>($"episodefile?episodeFileIds={fileIds}", HttpMethod.Get))!;

        foreach (Model_DownloadedEpisode episode in foundEpisodes)
            episode.filePath = files.FirstOrDefault(f => f.id == episode.fileId)?.path;

        return new Model_DownloadedMedia()
        {
            MediaId = mediaId,

            ProviderId = series.id,
            ProviderType = Domain.Enums.LibraryProviderType.Sonarr,
            SeasonId = matchedSeason.seasonNumber,

            ExternalQuality = series.qualityProfileId,
            ExternalRoot = series.rootFolderPath,

            IsMonitored = matchedSeason.monitored,
            LastChecked = DateTime.UtcNow,

            downloadedEpisodes = foundEpisodes
        };
    }

    public async Task<(string, List<int>)?> GetDownloadedMedia()
    {
        SonarrLibrary_Response_Series[]? series = await _http.SendRequest<SonarrLibrary_Response_Series[]>("series", HttpMethod.Get);

        if (series == null)
            return null;

        return (
            nameof(Model_Link.tvdb_id),
            series.Where(s => s.tvdbId > 0).Select(s => s.tvdbId).Distinct().ToList()
        );
    }

    public async Task<DownloadRequestInfo> GetRequestInfo(Model_Link link)
    {
        SonarrLibrary_Response_Roots[]? roots = await _http.SendRequest<SonarrLibrary_Response_Roots[]>("rootfolder", HttpMethod.Get);
        SonarrLibrary_Response_Qualities[]? qualities = await _http.SendRequest<SonarrLibrary_Response_Qualities[]>("qualitydefinition", HttpMethod.Get);

        return new DownloadRequestInfo()
        {
            sonarrRequestId = link.tvdb_id,
            sonarrSeasonId = link.tvdb_season,
            libraryProvider = LibraryProviderType.Sonarr,

            roots = roots?.Select(r => new DownloadRequestInfo.Roots()
            {
                path = r.path,
                freeSpace = r.freeSpace
            }).ToArray() ?? [],

            qualities = qualities?.Select(q => new DownloadRequestInfo.Qualities()
            {
                id = q.id,
                title = q.title
            }).ToArray() ?? [],
        };
    }

    public async Task<Model_DownloadedMedia?> RequestSeries(int mediaId, Model_DownloadedMedia? existing, DownloadRequest mediaRequest)
    {
        SonarrRequest_FetchLibrary? series = null;
        SonarrLibrary_Response_Episode[]? episodes = null;

        if (existing == null)
        {
            series = await _http.SendRequest<SonarrRequest_FetchLibrary>("series", HttpMethod.Post, new SonarrRequest_FetchLibrary()
            {
                tvdbId = mediaRequest.seriesId,
                title = Guid.NewGuid().ToString(),

                rootFolderPath = mediaRequest.rootPath,
                qualityProfileId = mediaRequest.qualityId,

                monitored = false,
                seasonFolder = true,

                addOptions = new SonarrRequest_FetchLibrary.AddOptions()
                {
                    searchForMissingEpisodes = false
                }
            });

            series = (await _http.SendRequest<SonarrRequest_FetchLibrary>(Path.Combine("series", series!.id.ToString()), HttpMethod.Get))!;
            series.monitored = true;

            foreach (SonarrLibrary_Response_Series.Seasons season in series.seasons!)
                season.monitored = false;

            await Task.Delay(1000); // :)))))
            await _http.SendRequest<SonarrRequest_FetchLibrary>(Path.Combine("series", series!.id.ToString()), HttpMethod.Put, series);

            return new Model_DownloadedMedia()
            {
                MediaId = mediaId,
                ProviderId = series.id,
                ProviderType = Domain.Enums.LibraryProviderType.Sonarr,
                SeasonId = mediaRequest.seasonId ?? -1,

                IsMonitored = true,

                ExternalQuality = series.qualityProfileId,
                ExternalRoot = series.rootFolderPath,

                LastChecked = DateTime.UtcNow,
                downloadedEpisodes = episodes!.Select(e => new Model_DownloadedEpisode()
                {
                    MediaId = mediaId,

                    Id = e.id,
                    EpisodeNumber = e.episodeNumber,
                    monitored = false,
                }).ToArray()
            };
        }

        series = await _http.SendRequest<SonarrRequest_FetchLibrary>(Path.Combine("series", existing.ProviderId.ToString()), HttpMethod.Get);

        if (series == null)
            throw new Exception("Couldnt find series");

        foreach (var season in series.seasons!)
            if (season.seasonNumber == mediaRequest.seasonId)
                season.monitored = mediaRequest.monitorSeason;

        existing.IsMonitored = mediaRequest.monitorSeason;
        series = await _http.SendRequest<SonarrRequest_FetchLibrary>(Path.Combine("series", series!.id.ToString()), HttpMethod.Put, series);

        SonarrLibrary_Response_Episode[]? eps = await _http.SendRequest<SonarrLibrary_Response_Episode[]>($"episode?seriesId={existing.ProviderId}&seasonNumber={mediaRequest.seasonId}", HttpMethod.Get);
        existing.downloadedEpisodes = eps.Select(e => new Model_DownloadedEpisode()
        {
            Id = e.id,
            MediaId = mediaId,
            EpisodeNumber = e.episodeNumber,
            monitored = e.monitored,
        }).ToArray();

        return existing;
    }

    public async Task<bool> ResearchMedia(Model_DownloadedMedia existingDownload)
    {
        try
        {
            await _http.SendRequest("command", HttpMethod.Post, new
            {
                name = "SeasonSearch",
                seriesId = existingDownload.ProviderId,
                seasonNumber = existingDownload.SeasonId
            });
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task DeleteMedia(Model_DownloadedMedia existingDownload)
    {
        int[] toDelete = existingDownload.downloadedEpisodes.Where(e => e.fileId.HasValue).Select(e => e.fileId!.Value).ToArray();
        await _http.SendRequest("episodefile/bulk", HttpMethod.Delete, new
        {
            episodeFileIds = toDelete
        });
    }
}

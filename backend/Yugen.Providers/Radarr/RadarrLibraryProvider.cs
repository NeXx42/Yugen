using System.Text.Json;
using System.Text.Json.Nodes;
using Yugen.Domain.Data.Downloads;
using Yugen.Domain.Enums;
using Yugen.Domain.Models.Library;
using Yugen.Domain.Models.Linking;
using Yugen.Providers.Helpers;
using Yugen.Providers.Sonarr;

namespace Yugen.Providers.Radarr;

public class RadarrLibraryProvider : ILibraryProvider
{
    private RestfulHelper _http;

    public RadarrLibraryProvider(string url, string apiKey)
    {
        _http = new RestfulHelper(Path.Combine(url, "api", "v3"), new Dictionary<string, string>()
        {
            { "X-Api-Key", apiKey }
        });
    }

    public Task DeleteMedia(Model_DownloadedMedia existingDownload)
    {
        throw new NotImplementedException();
    }

    public async Task<Model_DownloadedMedia?> GetDownloadedEpisodes(int mediaId, Model_Link link)
    {
        if (!link.themoviedb_id.HasValue)
            throw new Exception("Link doesnt contain tmdb id");

        RadarrLibrary_Response_Movie? res = (await _http.SendRequest<RadarrLibrary_Response_Movie[]>($"movie?tmdbId={link.themoviedb_id.Value}", HttpMethod.Get))?.FirstOrDefault();

        if (res == null)
            return null;

        List<Model_DownloadedEpisode> downloads = new List<Model_DownloadedEpisode>();

        if (res.movieFile != null)
        {
            downloads.Add(new Model_DownloadedEpisode()
            {
                Id = res.movieFile.id,
                EpisodeNumber = -1,
                MediaId = mediaId,
                fileId = res.movieFile.id,
                filePath = res.movieFile.path,
                monitored = true
            });
        }

        return new Model_DownloadedMedia()
        {
            MediaId = mediaId,
            ProviderId = link.themoviedb_id.Value,
            ProviderType = LibraryProviderType.Radarr,

            SeasonId = -1,
            ExternalRoot = res.rootFolderPath,
            ExternalQuality = res.qualityProfileId,
            LastChecked = DateTime.UtcNow,

            downloadedEpisodes = downloads
        };
    }

    public Task<List<int>?> GetDownloadedMedia()
    {
        throw new NotImplementedException();
    }

    public async Task<DownloadRequestInfo> GetRequestInfo(Model_Link link)
    {
        SonarrLibrary_Response_Roots[]? roots = await _http.SendRequest<SonarrLibrary_Response_Roots[]>("rootfolder", HttpMethod.Get);
        SonarrLibrary_Response_Qualities[]? qualities = await _http.SendRequest<SonarrLibrary_Response_Qualities[]>("qualitydefinition", HttpMethod.Get);

        return new DownloadRequestInfo()
        {
            sonarrRequestId = link.themoviedb_id,
            sonarrSeasonId = null,
            libraryProvider = LibraryProviderType.Radarr,

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

    public async Task<Model_DownloadedMedia?> RequestSeries(int mediaId, Model_DownloadedMedia? existingDownload, DownloadRequest request)
    {
        if (existingDownload != null)
        {
            throw new NotImplementedException();
        }

        RadarrLibrary_Request_Request[] req = [new RadarrLibrary_Request_Request()
        {
            tmdbId = request.seriesId,
            qualityProfileId = request.qualityId,
            rootFolderPath = request.rootPath,

            addOptions = new RadarrLibrary_Request_Request.AddOptions
            {
                addMethod = "manual",
                ignoreEpisodesWithFiles = true,
                ignoreEpisodesWithoutFiles = false,
                monitor = "movieOnly",
                searchForMovie = true
            }
        }];

        RadarrLibrary_Response_Movie? res = await _http.SendRequest<RadarrLibrary_Response_Movie>($"movie/import", HttpMethod.Post, req);

        if (res == null)
            throw new Exception("Failed to request?");

        return new Model_DownloadedMedia()
        {
            MediaId = mediaId,
            ProviderId = res.id,
            ProviderType = LibraryProviderType.Radarr,
            SeasonId = -1,
            ExternalQuality = res.qualityProfileId,
            ExternalRoot = res.rootFolderPath,
            LastChecked = DateTime.UtcNow,
            IsMonitored = true,
        };
    }

    public Task<bool> ResearchMedia(Model_DownloadedMedia existingDownload)
    {
        throw new NotImplementedException();
    }
}

using Yugen.Domain.Data.Media;
using Yugen.Domain.Models.Library;
using Yugen.Domain.Models.Linking;
using Yugen.Providers.Helpers;

namespace Yugen.Providers.Sonarr;

public class SonarrLibraryProvider : ILibraryProvider
{
    private RestfulHelper _http;

    public SonarrLibraryProvider(string url, string apiKey)
    {
        _http = new RestfulHelper(url, new Dictionary<string, string>()
        {
            { "X-Api-Key", apiKey }
        });
    }

    public async Task<List<Model_DownloadedEpisode>?> GetDownloadedEpisodes(int mediaId, Model_Link link)
    {
        SonarrLibrary_Response_Series[]? series = await _http.SendRequest<SonarrLibrary_Response_Series[]>("series");

        if (series == null)
            return null;

        SonarrLibrary_Response_Series? matchedEntry = null;

        foreach (SonarrLibrary_Response_Series entry in series)
        {
            if ((link.tvdb_id.HasValue && entry.tvdbId == link.tvdb_id.Value)
                || (link.themoviedb_id.HasValue && entry.tmdbId == link.themoviedb_id.Value))
            {
                matchedEntry = entry;
                break;
            }
        }

        if (matchedEntry == null)
            return null;

        SonarrLibrary_Response_Episode[]? episodes = await _http.SendRequest<SonarrLibrary_Response_Episode[]>($"episode?seriesId={matchedEntry.id}");

        if (episodes == null)
            return null;

        int seasonId = link.tvdb_season ?? link.tmdb_season ?? -1;

        Dictionary<Model_DownloadedEpisode, int?> foundEpisodes = new Dictionary<Model_DownloadedEpisode, int?>();

        foreach (SonarrLibrary_Response_Episode episode in episodes)
        {
            if (episode.seasonNumber == seasonId)
            {
                foundEpisodes.Add(new Model_DownloadedEpisode()
                {
                    MediaId = mediaId,
                    EpisodeNumber = episode.episodeNumber,

                    sonarrEpisodeId = episode.id,

                }, episode.episodeFileId == 0 ? null : episode.episodeFileId);
            }
        }

        if (foundEpisodes.Count == 0)
            return [];

        string fileIds = string.Join("&episodeFileIds=", foundEpisodes.Values.Where(x => x.HasValue));

        if (string.IsNullOrEmpty(fileIds))
            return [];

        SonarrLibrary_Response_EpisodeFile[]? files = (await _http.SendRequest<SonarrLibrary_Response_EpisodeFile[]>($"episodefile?episodeFileIds={fileIds}"))!;

        foreach (KeyValuePair<Model_DownloadedEpisode, int?> episode in foundEpisodes)
        {
            if (!episode.Value.HasValue)
                continue;

            SonarrLibrary_Response_EpisodeFile file = files.Single(f => f.id == episode.Value);
            episode.Key.filePath = file.path;
        }

        return foundEpisodes.Keys.ToList();
    }

    public async Task<List<int>?> GetDownloadedMedia()
    {
        SonarrLibrary_Response_Series[]? series = await _http.SendRequest<SonarrLibrary_Response_Series[]>("series");

        if (series == null)
            return null;

        return series.Where(s => s.tvdbId.HasValue).Select(s => s.tvdbId!.Value).Distinct().ToList();
    }

    public async Task RequestSeries(int tvdbId, int[] seasons, string rootFolder, int quality)
    {
        SonarrRequest_FetchLibrary request = new SonarrRequest_FetchLibrary()
        {
            tvdbId = tvdbId,
            title = Guid.NewGuid().ToString(),
            rootFolderPath = rootFolder,
            monitored = true,
            qualityProfileId = quality,
            seasonFolder = true,

            seasons = seasons.Select(s => new SonarrRequest_FetchLibrary.Season()
            {
                seasonNumber = s,
                monitored = true,
            }).ToArray(),

            addOptions = new SonarrRequest_FetchLibrary.AddOptions()
            {
                searchForMissingEpisodes = true
            }
        };

        SonarrLibrary_Response_Series? series = await _http.SendRequest<SonarrLibrary_Response_Series>("series", request);
    }
}

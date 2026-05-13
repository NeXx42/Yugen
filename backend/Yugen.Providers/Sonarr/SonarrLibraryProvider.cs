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

    public async Task<Model_DownloadedEpisode[]?> GetDownloadedEpisodes(int mediaId, Model_Link link)
    {
        SonarrLibrary_Response_Series[]? series = await _http.SendRequest<SonarrLibrary_Response_Series[]>("series");

        if (series == null)
            return null;

        SonarrLibrary_Response_Series? matchedEntry = null;

        foreach (SonarrLibrary_Response_Series entry in series)
        {
            if ((link.tvdbid.HasValue && entry.tvdbId == link.tvdbid.Value)
                || (link.tmdbtv.HasValue && entry.tmdbId == link.tmdbtv.Value))
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

        int seasonId = link.defaulttvdbseason ?? link.tmdbseason ?? -1;

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

        string fileIds = string.Join("&episodeFileIds=", foundEpisodes.Values.Where(x => x.HasValue));
        SonarrLibrary_Response_EpisodeFile[]? files = (await _http.SendRequest<SonarrLibrary_Response_EpisodeFile[]>($"episodefile?episodeFileIds={fileIds}"))!;

        foreach (KeyValuePair<Model_DownloadedEpisode, int?> episode in foundEpisodes)
        {
            if (!episode.Value.HasValue)
                continue;

            SonarrLibrary_Response_EpisodeFile file = files.Single(f => f.id == episode.Value);
            episode.Key.filePath = file.path;
        }

        return foundEpisodes.Keys.ToArray();
    }
}

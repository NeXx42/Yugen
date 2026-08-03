using Yugen.Core.Data;
using Yugen.Domain.Data;
using Yugen.Domain.Interfaces;
using Yugen.Domain.Models;
using Yugen.Domain.Models.Linking;
using Yugen.Domain.Models.Media;
using Yugen.Providers.Helpers;

namespace Yugen.Providers.Jikan;

public class JikanMetadataProvider : IMetaDataProvider
{
    private readonly RestfulHelper _http;

    public JikanMetadataProvider(ILogging logger)
    {
        _http = new RestfulHelper("https://api.jikan.moe/v4/", logger);
    }

    public async Task<Model_MediaEpisode[]> GetEpisodeData(Model_Link malId)
    {
        // api doesnt work ...
        JikanReponse_Episodes? episodes = await _http.SendRequest<JikanReponse_Episodes>(Path.Combine("anime", malId.mal_id!.Value.ToString(), "episodes"), HttpMethod.Get);

        if (episodes?.data == null)
            return [];

        return episodes.data.Select(e => new Model_MediaEpisode()
        {
            EpisodeNumber = e.mal_id,
            EpisodeTitle = e.title,

            IsFiller = e.filler,
            IsRecap = e.recap,

            Score = e.score,
        }).ToArray();
    }

    public Task<Model_Media[]> GetMediaInfo(MediaSearchQuery req)
    {
        throw new NotImplementedException();
    }

    public Task<(List<Model_Tag>, List<Model_Genre>)> GetSearchCriteria()
    {
        throw new NotImplementedException();
    }

    public Task<long?> GetTimeOfNextEpisode(int id)
    {
        throw new NotImplementedException();
    }

    public Task<Dictionary<int, long?>> GetTimeOfNextEpisodes(ICollection<int> aniListIds)
    {
        throw new NotImplementedException();
    }

    public Task<List<int>> GetTrending(int limit)
    {
        throw new NotImplementedException();
    }

    public Task<(int, int[])> SearchMedia(MediaSearchQuery req)
    {
        throw new NotImplementedException();
    }

    public Task<Dictionary<int, long>> UpcomingMedia()
    {
        throw new NotImplementedException();
    }
}

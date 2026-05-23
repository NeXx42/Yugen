using Yugen.Core.Data;
using Yugen.Domain.Data;
using Yugen.Domain.Models;
using Yugen.Domain.Models.Media;
using Yugen.Providers.Helpers;

namespace Yugen.Providers.Jikan;

public class JikanMetadataProvider : IMetaDataProvider
{
    private readonly RestfulHelper _http;

    public JikanMetadataProvider()
    {
        _http = new RestfulHelper("https://api.jikan.moe/v4/");
    }

    public async Task<Model_MediaEpisode[]> GetEpisodeData(int malId)
    {
        JikanReponse_Episodes? episodes = await _http.SendRequest<JikanReponse_Episodes>(Path.Combine("anime", malId.ToString(), "episodes"), HttpMethod.Get);

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

    public Task<Model_Media[]> GetMediaInfo(ICollection<int> aniListIds)
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

    public Task<List<int>> GetTrending(int limit)
    {
        throw new NotImplementedException();
    }

    public Task<(int, int[])> SearchMedia(MediaSearchQuery query, bool allowAdult)
    {
        throw new NotImplementedException();
    }

    public Task<Dictionary<int, long>> UpcomingMedia()
    {
        throw new NotImplementedException();
    }
}

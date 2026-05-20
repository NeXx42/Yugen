using Yugen.Core.Data;
using Yugen.Domain.Data;
using Yugen.Domain.Enums;
using Yugen.Domain.Models;
using Yugen.Domain.Models.Media;

namespace Yugen.Providers;

public interface IMetaDataProvider
{
    public Task<(List<Model_Tag>, List<Model_Genre>)> GetSearchCriteria();
    public Task<(int total, int[] ids)> SearchMedia(MediaSearchQuery query, bool allowAdult);

    public Task<List<int>> GetTrending(int limit);
    public Task<Dictionary<int, long>> UpcomingMedia();

    public Task<Model_Media[]> GetMediaInfo(ICollection<int> aniListIds);
    public Task<Model_MediaEpisode[]> GetEpisodeData(int malId);

    public Task<long?> GetTimeOfNextEpisode(int id);

}

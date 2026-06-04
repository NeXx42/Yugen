using Yugen.Core.Data;
using Yugen.Domain.Data;
using Yugen.Domain.Enums;
using Yugen.Domain.Models;
using Yugen.Domain.Models.Media;

namespace Yugen.Providers;

public interface IMetaDataProvider
{
    public Task<(List<Model_Tag>, List<Model_Genre>)> GetSearchCriteria();
    public Task<(int total, int[] ids)> SearchMedia(MediaSearchQuery filter);

    public Task<List<int>> GetTrending(int limit);
    public Task<Dictionary<int, long>> UpcomingMedia();

    public Task<Model_Media[]> GetMediaInfo(MediaSearchQuery filter);
    public Task<Model_MediaEpisode[]> GetEpisodeData(int malId);

    public Task<Dictionary<int, long?>> GetTimeOfNextEpisodes(ICollection<int> aniListIds);

}

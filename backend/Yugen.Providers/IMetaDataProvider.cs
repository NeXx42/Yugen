using Yugen.Core.Data;
using Yugen.Domain.Enums;
using Yugen.Domain.Models.Media;

namespace Yugen.Providers;

public interface IMetaDataProvider
{
    public Task<int[]> SearchMedia(string textFilter);
    public Task<Dictionary<int, long>> UpcomingMedia();

    public Task<Model_Media[]> GetMediaInfo(ICollection<int> aniListIds);
    public Task<Model_MediaEpisode[]> GetEpisodeData(int malId);
}

using System.Collections.ObjectModel;
using Yugen.Domain.Models.History;
using Yugen.Domain.Models.Library;

namespace Yugen.Providers;

public interface IMediaProvider
{
    public Task<string> Play();
    public Task<string?[]?> MapPathToJellyfinId(ICollection<Model_DownloadedEpisode> episodes);

    public Task<Model_WatchedEpisode[]> UpdateWatchHistory(string userId, ICollection<Model_DownloadedEpisode> media);
}

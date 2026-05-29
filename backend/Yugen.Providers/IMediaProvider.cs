using System.Collections.ObjectModel;
using Yugen.Domain.Data.Media;
using Yugen.Domain.Models.History;
using Yugen.Domain.Models.Library;

namespace Yugen.Providers;

public interface IMediaProvider
{
    public Task<PlaybackInfo> GetPlaybackInfo(string jellyfinId);

    public Task DeleteSubtitle(string jellyfinId, int id);
    public Task UploadSubtitle(string jellyfinId, string language, string format, string data);

    public Task<string> ProxyUrl(string relative, bool includeApiKey = false);
    public Task<string> GetPlaybackUrl(string jellyfinId, int source, bool hls, string? videoCodecs, string? audioCodecs);
    public Task<string> GetSubtitleUrl(string jellyfinId, string mediaId, int subtitleId);

    public Task<string?[]?> MapPathToJellyfinId(ICollection<Model_DownloadedEpisode> episodes);

    public Task<Model_WatchedEpisode[]> UpdateWatchHistory(string userId, ICollection<Model_DownloadedEpisode> media);
}

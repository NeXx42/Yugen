using System.Collections.ObjectModel;
using Yugen.Domain.Data.Downloads;
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
    public Task<string> GetSubtitleUrl(string jellyfinId, string mediaId, int subtitleId);
    public Task<DownloadedEpisodeSubtitles[]> GetSubtitles(ICollection<string> jellyfinIds);
    public Task<string> GetPlaybackUrl(string jellyfinId, int source, bool hls, long? maxBitrate, string? videoCodecs, string? audioCodecs, int? audioIndex);

    public Task<string?[]?> MapPathToJellyfinId(ICollection<Model_DownloadedEpisode> episodes);
}

namespace Yugen.Providers.Jellyfin;

public class JellyfinResponse_Page<T>
{
    public required T[] Items { get; set; }
    public int TotalRecordCount { get; set; }
    public int StartIndex { get; set; }
}


public class JellyfinResponse_Session
{
    public string? AccessToken { get; set; }
    public string? ServerId { get; set; }
    public required JellyfinResponse_User User { get; set; }
}

public class JellyfinResponse_User
{
    public string? Name { get; set; }
    public string? ServerId { get; set; }
    public string? ServerName { get; set; }
    public string? Id { get; set; }
}



public class JellyfinResponse_Media
{
    public string? name { get; set; }
    public string? id { get; set; }
    public string? type { get; set; }
    public string? seasonId { get; set; }
    public string? seriesId { get; set; }
    public string? seriesName { get; set; }
    public int? indexNumber { get; set; }

    public ProviderIds? providerIds { get; set; }

    public class ProviderIds
    {
        public string? AniList { get; set; }
    }
}

public class Jellyfin_Response_Item
{
    public string? id { get; set; }
    public string? serverId { get; set; }
    public string? path { get; set; }
}

public class Jellyfin_Response_History
{
    public required string id { get; set; }
    public long? runTimeTicks { get; set; }
    public UserData? userData { get; set; }

    public class UserData
    {
        public long playBackPositionTicks { get; set; }
        public int playCount { get; set; }
        public bool isFavorite { get; set; }
        public bool played { get; set; }
        public DateTime? LastPlayedDate { get; set; }
    }
}

public class JellyfinResponse_MediaInfo
{
    public MediaSource[]? MediaSources { get; set; }

    public class MediaSource
    {
        public string? id { get; set; }
        public string? protocol { get; set; }

        public string? path { get; set; }
        public string? type { get; set; }
        public string? container { get; set; }

        public long? size { get; set; }
        public string? Name { get; set; }
        public bool? IsRemote { get; set; }
        public string? ETag { get; set; }
        public long? RunTimeTicks { get; set; }
        public bool? ReadAtNativeFramerate { get; set; }
        public bool? IgnoreDts { get; set; }
        public bool? IgnoreIndex { get; set; }
        public bool? GenPtsInput { get; set; }
        public bool? SupportsTranscoding { get; set; }
        public bool? SupportsDirectStream { get; set; }
        public bool? SupportsDirectPlay { get; set; }
        public bool? IsInfiniteStream { get; set; }
        public bool? UseMostCompatibleTranscodingProfile { get; set; }
        public bool? RequiresOpening { get; set; }
        public bool? RequiresClosing { get; set; }
        public bool? RequiresLooping { get; set; }
        public bool? SupportsProbing { get; set; }
        public string? VideoType { get; set; }

        public long? Bitrate { get; set; }
        public string? TranscodingSubProtocol { get; set; }
        public bool? HasSegments { get; set; }

        public MediaStream[]? MediaStreams { get; set; }
        public MediaAttachment[]? MediaAttachments { get; set; }

        public class MediaStream
        {
            public string? Codec { get; set; }
            public string? Language { get; set; }
            public string? ColorSpace { get; set; }
            public string? ColorTransfer { get; set; }
            public string? ColorPrimaries { get; set; }
            public string? TimeBase { get; set; }
            public string? Title { get; set; }
            public string? VideoRange { get; set; }
            public string? VideoRangeType { get; set; }
            public string? AudioSpatialFormat { get; set; }
            public string? DisplayTitle { get; set; }
            public string? NalLengthSize { get; set; }
            public bool? IsInterlaced { get; set; }
            public bool? IsAVC { get; set; }
            public long? BitRate { get; set; }
            public uint? BitDepth { get; set; }
            public uint? RefFrames { get; set; }
            public bool? IsDefault { get; set; }
            public bool? IsForced { get; set; }
            public bool? IsHearingImpaired { get; set; }
            public uint? Height { get; set; }
            public uint? Width { get; set; }
            public decimal? AverageFrameRate { get; set; }
            public decimal? RealFrameRate { get; set; }
            public decimal? ReferenceFrameRate { get; set; }
            public string? Profile { get; set; }
            public string? Type { get; set; }
            public string? AspectRatio { get; set; }
            public required int Index { get; set; }
            public bool? IsExternal { get; set; }
            public bool? IsTextSubtitleStream { get; set; }
            public bool? SupportsExternalStream { get; set; }
            public string? PixelFormat { get; set; }
            public int? Level { get; set; }
            public bool? IsAnamorphic { get; set; }
        }

        public class MediaAttachment
        {
            public string? Codec { get; set; }
            public int? Index { get; set; }
            public string? FileName { get; set; }
            public string? MimeType { get; set; }
        }
    }
}
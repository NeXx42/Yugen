using System.Net.NetworkInformation;

namespace Yugen.Providers.Tenrai;

public class TenraiMetadata_Responses_Container<T>
{
    public T? data { get; set; }
}

public class TenraiMetadata_Responses_Page<T>
{
    public Pagination? pagination { get; set; }
    public T[]? data { get; set; }

    public struct Pagination
    {
        public int? last_visible_page { get; set; }
        public bool? has_next_page { get; set; }
        public int? current_page { get; set; }
        public Items items { get; set; }

        public struct Items
        {
            public int? count { get; set; }
            public int? total { get; set; }
            public int? per_page { get; set; }
        }
    }
}

public class TenraiMetadata_Responses_Anime
{
    public int mal_id { get; set; }
    public string? url { get; set; }

    public Trailer? trailer { get; set; }
    public Images? images { get; set; }

    public string? title { get; set; }
    public string? title_english { get; set; }
    public string? title_japanese { get; set; }

    public string? type { get; set; }
    public int? episodes { get; set; }

    public string? status { get; set; }

    public float? score { get; set; }
    public int? rank { get; set; }
    public int? popularity { get; set; }

    public string? synopsis { get; set; }
    public string? background { get; set; }

    public int? year { get; set; }
    public string? season { get; set; }

    public bool airing { get; set; }
    public Aired? aired { get; set; }
    public Broadcast? broadcast { get; set; }

    public Genres[]? genres { get; set; }

    public long? getNextEpisodeDate
    {
        get
        {
            if (!airing)
                return null;

            if (broadcast?.day != null && broadcast?.time != null && !string.IsNullOrEmpty(broadcast?.timezone))
            {
                TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(broadcast.Value.timezone!);

                DateTimeOffset nowUtc = DateTimeOffset.UtcNow;
                DateTime nowInZone = TimeZoneInfo.ConvertTime(nowUtc, timeZone).DateTime;

                TimeOnly scheduledTime = TimeOnly.Parse(broadcast.Value.time!);

                DayOfWeek targetDay = Enum.Parse<DayOfWeek>(broadcast.Value.day!.TrimEnd('s'), ignoreCase: true);
                int daysUntil = ((int)targetDay - (int)nowInZone.DayOfWeek + 7) % 7;

                DateTime nextDate = nowInZone.Date.AddDays(daysUntil);
                DateTime nextLocal = nextDate.Add(scheduledTime.ToTimeSpan());

                if (nextLocal <= nowInZone)
                    nextLocal = nextLocal.AddDays(7);

                nextLocal = DateTime.SpecifyKind(nextLocal, DateTimeKind.Unspecified);
                DateTime nextUtc = TimeZoneInfo.ConvertTimeToUtc(nextLocal, timeZone);

                return new DateTimeOffset(nextUtc).ToUnixTimeSeconds();
            }

            return null;
        }
    }

    public long? getAiredFrom => (aired?.prop?.from.HasValue ?? false) ? null
        : new DateTimeOffset(aired!.Value.prop!.Value.from!.Value.year!.Value, aired!.Value.prop!.Value.from!.Value.month ?? 1, aired!.Value.prop!.Value.from!.Value.day ?? 1, 0, 0, 0, TimeSpan.MinValue).ToUnixTimeSeconds();

    public long? getAiredTo => (aired?.prop?.to.HasValue ?? false) ? null
        : new DateTimeOffset(aired!.Value.prop!.Value.to!.Value.year!.Value, aired!.Value.prop!.Value.to!.Value.month ?? 1, aired!.Value.prop!.Value.to!.Value.day ?? 1, 0, 0, 0, TimeSpan.MinValue).ToUnixTimeSeconds();


    public struct Images
    {
        public Webp? webp { get; set; }

        public struct Webp
        {
            public string image_url { get; set; }
            public string small_image_url { get; set; }
            public string large_image_url { get; set; }
        }
    }

    public struct Trailer
    {
        public Images? images { get; set; }

        public struct Images
        {
            public string? getBestBanner => maximum_image_url ?? large_image_url ?? medium_image_url ?? small_image_url ?? image_url;

            public string? image_url { get; set; }
            public string? small_image_url { get; set; }
            public string? medium_image_url { get; set; }
            public string? large_image_url { get; set; }
            public string? maximum_image_url { get; set; }
        }
    }

    public struct Broadcast
    {
        public string? day { get; set; }
        public string? time { get; set; }
        public string? timezone { get; set; }
    }

    public struct Aired
    {
        public Prop? prop { get; set; }

        public struct Prop
        {
            public Prop_Comps? to { get; set; }
            public Prop_Comps? from { get; set; }

            public struct Prop_Comps
            {
                public int? day { get; set; }
                public int? month { get; set; }
                public int? year { get; set; }
            }
        }
    }

    public struct Genres
    {
        public int mal_id { get; set; }
        public string? type { get; set; }
        public string? name { get; set; }
        public string? url { get; set; }
    }
}


public struct TenraiMetadata_Responses_Episode
{
    public int mal_id { get; set; }

    public string? title { get; set; }
    public string? title_japanese { get; set; }

    public int? duration { get; set; }
    public float? score { get; set; }

    public bool? filler { get; set; }
    public bool? recap { get; set; }

    public string? synopsis { get; set; }

    public string? aired { get; set; }
    public Images? images { get; set; }

    public struct Images
    {
        public Jpeg? jpeg { get; set; }

        public struct Jpeg
        {
            public string image_url { get; set; }
        }
    }
}

public struct TenraiMetadata_Responses_Recommended
{
    public Entry entry { get; set; }
    public int votes { get; set; }

    public struct Entry
    {
        public int mal_id { get; set; }
    }
}
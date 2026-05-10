namespace Yugen.Providers.Jellyfin;

public class JellyfinResponse_Page<T>
{
    public T[] Items { get; set; }
    public int TotalRecordCount { get; set; }
    public int StartIndex { get; set; }
}


public class JellyfinResponse_Session
{
    public string? AccessToken { get; set; }
    public string? ServerId { get; set; }
    public JellyfinResponse_User? User { get; set; }
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
    public ProviderIds? providerIds { get; set; }

    public class ProviderIds
    {
        public string? AniList { get; set; }
    }
}
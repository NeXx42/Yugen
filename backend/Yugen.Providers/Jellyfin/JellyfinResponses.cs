namespace Yugen.Providers.Jellyfin;

public class JellyfinResponse_Session
{
    public string AccessToken { get; set; }
    public string ServerId { get; set; }
    public JellyfinResponse_User User { get; set; }
}

public class JellyfinResponse_User
{
    public string Name { get; set; }
    public string ServerId { get; set; }
    public string ServerName { get; set; }
    public string Id { get; set; }
}
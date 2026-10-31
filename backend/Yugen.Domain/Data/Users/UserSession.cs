namespace Yugen.Domain.Data.Users;

public class UserSession
{
    public Guid Id { get; set; }
    public string? Name { get; set; }

    public Dictionary<string, string> Settings { get; set; } = new Dictionary<string, string>();
}

namespace Yugen.Domain.Data.Users;

public class JWTUser
{
    public Guid Id { get; set; }
    public required string JellyfinId { get; set; }

    public static JWTUser SystemStandIn => new JWTUser() { Id = Guid.Empty, JellyfinId = Guid.Empty.ToString() };
}

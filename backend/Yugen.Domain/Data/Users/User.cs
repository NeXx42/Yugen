namespace Yugen.Domain.Data.Users;

public class ExternalUser
{
    public required string Name { get; set; }
    public string? ExternalId { get; set; }
}

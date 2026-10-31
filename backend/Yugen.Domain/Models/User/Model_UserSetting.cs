using System.ComponentModel.DataAnnotations;

namespace Yugen.Domain.Models.User;

public class Model_UserSetting
{
    public Guid UserId { get; set; }
    public UserModel User { get; set; } = null!;

    [Required]
    public required string Key { get; set; }

    [Required]
    public required string Value { get; set; }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yugen.Domain.Models.User;

namespace Yugen.Domain.Models;

public class UserModel
{
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required]
    public required string ProviderId { get; set; }

    public ICollection<Model_UserSetting> settings = new List<Model_UserSetting>();
}

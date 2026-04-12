using Microsoft.AspNetCore.Identity;

namespace Identity.Entities;

public class User : IdentityUser<Guid>
{
  public string? DisplayName { get; set; }
}

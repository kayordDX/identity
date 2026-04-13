using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Identity.Entities;

namespace Identity.Data.Config;

public class IdentityUserRoleConfig : IEntityTypeConfiguration<UserRole>
{
  public void Configure(EntityTypeBuilder<UserRole> builder)
  {
    builder.ToTable("user_role");
    builder.HasKey(ur => new { ur.UserId, ur.RoleId });
  }
}

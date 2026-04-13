using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Data.Config;

public class IdentityRoleClaimConfig : IEntityTypeConfiguration<IdentityRoleClaim<Guid>>
{
  public void Configure(EntityTypeBuilder<IdentityRoleClaim<Guid>> builder)
  {
    builder.ToTable("role_claim");
  }
}

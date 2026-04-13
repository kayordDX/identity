using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenIddict.EntityFrameworkCore.Models;

namespace Identity.Data.Config;

public class OpenIddictAuthorizationConfig : IEntityTypeConfiguration<OpenIddictEntityFrameworkCoreAuthorization>
{
  public void Configure(EntityTypeBuilder<OpenIddictEntityFrameworkCoreAuthorization> builder)
  {
    builder.ToTable("openiddict_authorization");
  }
}

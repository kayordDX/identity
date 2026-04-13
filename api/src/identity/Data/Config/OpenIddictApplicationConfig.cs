using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenIddict.EntityFrameworkCore.Models;

namespace Identity.Data.Config;

public class OpenIddictApplicationConfig : IEntityTypeConfiguration<OpenIddictEntityFrameworkCoreApplication>
{
  public void Configure(EntityTypeBuilder<OpenIddictEntityFrameworkCoreApplication> builder)
  {
    builder.ToTable("openiddict_application");
  }
}

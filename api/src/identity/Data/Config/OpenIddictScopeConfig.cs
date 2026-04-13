using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenIddict.EntityFrameworkCore.Models;

namespace Identity.Data.Config;

public class OpenIddictScopeConfig : IEntityTypeConfiguration<OpenIddictEntityFrameworkCoreScope>
{
  public void Configure(EntityTypeBuilder<OpenIddictEntityFrameworkCoreScope> builder)
  {
    builder.ToTable("openiddict_scope");
  }
}

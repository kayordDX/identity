using Identity.Data;
using Quartz;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Identity.Common.Extensions;

public static class OpenIddictExtensions
{
  public static IServiceCollection ConfigureQuartz(this IServiceCollection services)
  {
    services.AddQuartz(options =>
    {
      options.UseSimpleTypeLoader();
      options.UseInMemoryStore();
    });
    services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);
    return services;
  }

  public static IServiceCollection ConfigureOpenIddict(this IServiceCollection services)
  {
    services.AddOpenIddict()
      .AddCore(options =>
      {
        options.UseEntityFrameworkCore().UseDbContext<AppDbContext>();
        options.UseQuartz();
      })
      .AddServer(options =>
      {
        options
          .SetAuthorizationEndpointUris("connect/authorize")
          .SetTokenEndpointUris("connect/token")
          .SetUserInfoEndpointUris("connect/userinfo")
          .SetEndSessionEndpointUris("connect/logout");

        options
          .AllowAuthorizationCodeFlow().RequireProofKeyForCodeExchange()
          .AllowRefreshTokenFlow();

        options.RegisterScopes(
          Scopes.OpenId,
          Scopes.Profile,
          Scopes.Email,
          Scopes.OfflineAccess);

        options
          .AddDevelopmentEncryptionCertificate()
          .AddDevelopmentSigningCertificate();

        options.UseAspNetCore()
          .DisableTransportSecurityRequirement() // Allow HTTP in development
          .EnableAuthorizationEndpointPassthrough()
          .EnableEndSessionEndpointPassthrough()
          .EnableUserInfoEndpointPassthrough()
          .EnableStatusCodePagesIntegration();
      })
      .AddValidation(options =>
      {
        options.UseLocalServer();
        options.UseAspNetCore();
      });
    return services;
  }
}

using System.Security.Cryptography.X509Certificates;
using Identity.Common.Config;
using Identity.Data;
using Microsoft.IdentityModel.Tokens;
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

  public static IServiceCollection ConfigureOpenIddict(this IServiceCollection services, IConfiguration configuration)
  {
    var appConfig = configuration.GetSection("App").Get<AppConfig>()
        ?? throw new InvalidOperationException("App configuration is missing.");

    services.AddOpenIddict()
      .AddCore(options =>
      {
        options.UseEntityFrameworkCore().UseDbContext<AppDbContext>();
        options.UseQuartz();
      })
      .AddServer(options =>
      {
        options
          .SetAccessTokenLifetime(TimeSpan.FromMinutes(appConfig.AccessTokenLifetime))
          .SetRefreshTokenLifetime(TimeSpan.FromDays(appConfig.RefreshTokenLifetime));

        options
          .SetAuthorizationEndpointUris("connect/authorize")
          .SetTokenEndpointUris("connect/token")
          .SetUserInfoEndpointUris("connect/userinfo")
          .SetEndSessionEndpointUris("connect/logout");

        options
          .AllowAuthorizationCodeFlow().RequireProofKeyForCodeExchange()
          .AllowPasswordFlow()
          .AllowRefreshTokenFlow();

        options.RegisterScopes(
          Scopes.OpenId,
          Scopes.Profile,
          Scopes.Email,
          Scopes.OfflineAccess);

        options.AddEncryptionKey(new SymmetricSecurityKey(Convert.FromBase64String(appConfig.EncryptionKey)));
        var pfx = X509CertificateLoader.LoadPkcs12FromFile(appConfig.SigningCertPath, appConfig.SigningCertPassword);
        options.AddSigningCertificate(pfx);

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

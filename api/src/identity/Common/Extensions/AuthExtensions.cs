using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Validation.AspNetCore;

namespace Identity.Common.Extensions;

public static class AuthExtensions
{
  public static IServiceCollection ConfigureAuth(this IServiceCollection services)
  {
    services.AddAuthorization(ConfigureAuthorization);
    services.ConfigureApplicationCookie(options =>
    {
      options.LoginPath = "/account/login";
    });
    return services;
  }

  static void ConfigureAuthorization(AuthorizationOptions options)
  {
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(IdentityConstants.ApplicationScheme, OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .Build();
  }

  public static IServiceCollection ConfigureGoogleAuth(this IServiceCollection services, IConfiguration configuration)
  {
    var googleClientId = configuration["Authentication:Google:ClientId"];
    var googleClientSecret = configuration["Authentication:Google:ClientSecret"];

    if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
    {
      services.AddAuthentication()
        .AddGoogle(options =>
        {
          options.ClientId = googleClientId;
          options.ClientSecret = googleClientSecret;
          options.Events.OnCreatingTicket = ctx =>
          {
            var picture = ctx.User.GetProperty("picture").GetString();
            if (!string.IsNullOrEmpty(picture))
              ctx.Identity?.AddClaim(new Claim("picture", picture));
            return Task.CompletedTask;
          };
        });
    }
    return services;
  }
}

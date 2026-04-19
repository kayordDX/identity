using Microsoft.AspNetCore.Authentication;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using OidcClaims = OpenIddict.Abstractions.OpenIddictConstants.Claims;
using OidcScopes = OpenIddict.Abstractions.OpenIddictConstants.Scopes;

namespace Identity.Features.Connect.UserInfo;

public class UserInfoEndpoint : EndpointWithoutRequest<IDictionary<string, object?>>
{
  public override void Configure()
  {
    Get("connect/userinfo");
    Description(x => x.WithName("UserInfo"));
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

    if (!result.Succeeded)
    {
      await Send.ResultAsync(Results.Forbid(authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]));
      return;
    }

    var principal = result.Principal!;
    var claims = new Dictionary<string, object?>();

    if (principal.HasScope(OidcScopes.OpenId))
      claims[OidcClaims.Subject] = principal.GetClaim(OidcClaims.Subject);

    if (principal.HasScope(OidcScopes.Profile))
      claims[OidcClaims.Name] = principal.GetClaim(OidcClaims.Name);

    if (principal.HasScope(OidcScopes.Email))
      claims[OidcClaims.Email] = principal.GetClaim(OidcClaims.Email);

    await Send.ResultAsync(Results.Json(
      claims.Where(kv => kv.Value is not null).ToDictionary(kv => kv.Key, kv => kv.Value)));
  }
}

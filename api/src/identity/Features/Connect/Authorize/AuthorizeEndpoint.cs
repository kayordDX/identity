using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Identity.Features.Connect.Authorize;

public class GetAuthorizeEndpoint : EndpointWithoutRequest<bool>
{
  public override void Configure()
  {
    Get("connect/authorize");
    AllowAnonymous();
    Description(x => x.WithName("GetAuthorize"));
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var request = HttpContext.GetOpenIddictServerRequest()
      ?? throw new InvalidOperationException("The OIDC request cannot be retrieved.");

    // Check whether the user is already signed in via Identity's cookie
    var result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);

    if (!result.Succeeded)
    {
      await Send.ResultAsync(Results.Challenge(
        new AuthenticationProperties { RedirectUri = HttpContext.Request.GetEncodedUrl() },
        [IdentityConstants.ApplicationScheme]));
      ResponseStarted = true;
      return;
    }

    // Extract claims from the Identity cookie
    var cookiePrincipal = result.Principal;
    var userId = cookiePrincipal.FindFirstValue(ClaimTypes.NameIdentifier)!;
    var userName = cookiePrincipal.FindFirstValue(ClaimTypes.Name) ?? userId;
    var email = cookiePrincipal.FindFirstValue(ClaimTypes.Email);

    // Build an OpenIddict identity with the right claim destinations
    var identity = new ClaimsIdentity(
      TokenValidationParameters.DefaultAuthenticationType,
      OpenIddictConstants.Claims.Name,
      OpenIddictConstants.Claims.Role
    );

    identity
      .SetClaim(OpenIddictConstants.Claims.Subject, userId)
      .SetClaim(OpenIddictConstants.Claims.Name, userName)
      .SetClaim(OpenIddictConstants.Claims.Email, email);

    var principal = new ClaimsPrincipal(identity);
    principal.SetScopes(request.GetScopes());

    foreach (var claim in principal.Claims)
      claim.SetDestinations(GetClaimDestinations(claim, principal));

    await Send.ResultAsync(Results.SignIn(
      principal,
      properties: null,
      OpenIddictServerAspNetCoreDefaults.AuthenticationScheme));
  }

  internal static IEnumerable<string> GetClaimDestinations(Claim claim, ClaimsPrincipal principal)
  {
    if (claim.Type == OpenIddictConstants.Claims.Name || claim.Type == OpenIddictConstants.Claims.PreferredUsername)
      return principal.HasScope(OpenIddictConstants.Claims.Profile)
        ? [Destinations.AccessToken, Destinations.IdentityToken]
        : [Destinations.AccessToken];

    if (claim.Type == OpenIddictConstants.Claims.Email)
      return principal.HasScope(OpenIddictConstants.Claims.Email)
        ? [Destinations.AccessToken, Destinations.IdentityToken]
        : [Destinations.AccessToken];

    if (claim.Type == OpenIddictConstants.Claims.Subject)
      return [Destinations.AccessToken, Destinations.IdentityToken];

    return [Destinations.AccessToken];
  }
}

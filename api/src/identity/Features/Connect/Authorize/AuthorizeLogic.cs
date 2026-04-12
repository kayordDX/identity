using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using OidcClaims = OpenIddict.Abstractions.OpenIddictConstants.Claims;
using OidcDestinations = OpenIddict.Abstractions.OpenIddictConstants.Destinations;
using OidcScopes = OpenIddict.Abstractions.OpenIddictConstants.Scopes;

namespace Identity.Features.Connect.Authorize;

internal static class AuthorizeLogic
{
  internal static async Task HandleAsync(HttpContext ctx, Func<IResult, Task> send)
  {
    var oidcRequest = ctx.GetOpenIddictServerRequest()
      ?? throw new InvalidOperationException("The OpenIddict server request cannot be retrieved.");

    // Check whether the user is already signed in via Identity's cookie
    var authResult = await ctx.AuthenticateAsync(IdentityConstants.ApplicationScheme);

    if (authResult?.Principal is not { Identity.IsAuthenticated: true })
    {
      // Not signed in – redirect to login page, then back here
      await send(Results.Challenge(
        new AuthenticationProperties { RedirectUri = ctx.Request.GetEncodedUrl() },
        [IdentityConstants.ApplicationScheme]));
      return;
    }

    // Extract claims from the Identity cookie
    var cookiePrincipal = authResult.Principal;
    var userId = cookiePrincipal.FindFirstValue(ClaimTypes.NameIdentifier)!;
    var userName = cookiePrincipal.FindFirstValue(ClaimTypes.Name) ?? userId;
    var email = cookiePrincipal.FindFirstValue(ClaimTypes.Email);

    // Build an OpenIddict identity with the right claim destinations
    var identity = new ClaimsIdentity(
      authenticationType: TokenValidationParameters.DefaultAuthenticationType,
      nameType: OidcClaims.Name,
      roleType: OidcClaims.Role);

    identity
      .SetClaim(OidcClaims.Subject, userId)
      .SetClaim(OidcClaims.Name, userName)
      .SetClaim(OidcClaims.Email, email);

    var principal = new ClaimsPrincipal(identity);
    principal.SetScopes(oidcRequest.GetScopes());

    foreach (var claim in principal.Claims)
      claim.SetDestinations(GetClaimDestinations(claim, principal));

    await send(Results.SignIn(
      principal,
      properties: null,
      OpenIddictServerAspNetCoreDefaults.AuthenticationScheme));
  }

  internal static IEnumerable<string> GetClaimDestinations(Claim claim, ClaimsPrincipal principal)
  {
    if (claim.Type == OidcClaims.Name || claim.Type == OidcClaims.PreferredUsername)
      return principal.HasScope(OidcScopes.Profile)
        ? [OidcDestinations.AccessToken, OidcDestinations.IdentityToken]
        : [OidcDestinations.AccessToken];

    if (claim.Type == OidcClaims.Email)
      return principal.HasScope(OidcScopes.Email)
        ? [OidcDestinations.AccessToken, OidcDestinations.IdentityToken]
        : [OidcDestinations.AccessToken];

    if (claim.Type == OidcClaims.Subject)
      return [OidcDestinations.AccessToken, OidcDestinations.IdentityToken];

    return [OidcDestinations.AccessToken];
  }
}

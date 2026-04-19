using System.Security.Claims;
using Identity.Data;
using Identity.Entities;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Identity.Features.Connect.Authorize;

public class GetAuthorizeEndpoint(SignInManager<User> signInManager, UserManager<User> userManager) : EndpointWithoutRequest<bool>
{
  private readonly SignInManager<User> _signInManager = signInManager;
  private readonly UserManager<User> _userManager = userManager;

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

    var user = await _userManager.GetUserAsync(result.Principal);
    if (user is null)
    {
      await Send.ErrorsAsync(401, ct);
      return;
    }

    var principal = await _signInManager.CreateUserPrincipalAsync(user);
    var userId = await _userManager.GetUserIdAsync(user);
    principal.SetClaim(OpenIddictConstants.Claims.Subject, userId);
    principal.SetClaim(OpenIddictConstants.Claims.Email, user.Email);
    principal.SetScopes(request.GetScopes());

    foreach (var claim in principal.Claims)
    {
      claim.SetDestinations(Destinations.AccessToken, Destinations.IdentityToken);
    }

    await Send.ResultAsync(Results.SignIn(
      principal,
      authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme));
  }
}

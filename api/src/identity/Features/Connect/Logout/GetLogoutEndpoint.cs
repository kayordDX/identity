using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Server.AspNetCore;

namespace Identity.Features.Connect.Logout;

public class GetLogoutEndpoint : EndpointWithoutRequest<bool>
{
  public override void Configure()
  {
    Get("connect/logout");
    AllowAnonymous();
    Description(x => x.WithName("GetLogout"));
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

    await Send.ResultAsync(Results.SignOut(
      properties: null,
      authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]));
  }
}

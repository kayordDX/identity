using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Identity.Entities;

namespace Identity.Features.Account.Login;

public class ExternalLoginEndpoint(SignInManager<User> signInManager) : EndpointWithoutRequest
{
  public override void Configure()
  {
    Get("/account/login/external");
    AllowAnonymous();
    Description(x => x.WithName("ExternalLogin"));
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var provider = HttpContext.Request.Query["provider"].FirstOrDefault();
    var returnUrl = HttpContext.Request.Query["returnUrl"].FirstOrDefault() ?? "/";

    if (string.IsNullOrWhiteSpace(provider))
    {
      await Send.ResultAsync(Results.BadRequest("A provider must be specified."));
      return;
    }

    var redirectUrl = $"/account/login/external-callback?returnUrl={Uri.EscapeDataString(returnUrl)}";
    var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

    await HttpContext.ChallengeAsync(provider, properties);
  }
}

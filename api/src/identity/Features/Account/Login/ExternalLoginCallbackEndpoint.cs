using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Identity.Entities;

namespace Identity.Features.Account.Login;

public class ExternalLoginCallbackEndpoint(
  SignInManager<User> signInManager,
  UserManager<User> userManager) : EndpointWithoutRequest
{
  public override void Configure()
  {
    Get("/account/login/external-callback");
    AllowAnonymous();
    Description(x => x.WithName("ExternalLoginCallback"));
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var returnUrl = HttpContext.Request.Query["returnUrl"].FirstOrDefault() ?? "/";
    var loginUrl = $"/login?returnUrl={Uri.EscapeDataString(returnUrl)}";

    var info = await signInManager.GetExternalLoginInfoAsync();
    if (info is null)
    {
      await Send.ResultAsync(Results.Redirect(
        loginUrl + "&error=" + Uri.EscapeDataString("Google sign-in failed. Please try again.")));
      return;
    }

    // Happy path – user already has a linked external login
    var signInResult = await signInManager.ExternalLoginSignInAsync(
      info.LoginProvider, info.ProviderKey,
      isPersistent: false, bypassTwoFactor: true);

    if (signInResult.Succeeded)
    {
      await Send.ResultAsync(Results.Redirect(returnUrl));
      return;
    }

    // No linked login yet – resolve or create a local user
    var email = info.Principal.FindFirstValue(ClaimTypes.Email);
    if (string.IsNullOrWhiteSpace(email))
    {
      await Send.ResultAsync(Results.Redirect(
        loginUrl + "&error=" + Uri.EscapeDataString("No email address was returned by Google.")));
      return;
    }

    var user = await userManager.FindByEmailAsync(email);

    if (user is null)
    {
      var displayName =
        info.Principal.FindFirstValue(ClaimTypes.Name) ??
        info.Principal.FindFirstValue(ClaimTypes.GivenName) ??
        email;

      user = new User
      {
        UserName = email,
        Email = email,
        EmailConfirmed = true,   // trusted – came from Google
        DisplayName = displayName,
      };

      var createResult = await userManager.CreateAsync(user);
      if (!createResult.Succeeded)
      {
        var errors = string.Join(" ", createResult.Errors.Select(e => e.Description));
        await Send.ResultAsync(Results.Redirect(
          loginUrl + "&error=" + Uri.EscapeDataString(errors)));
        return;
      }
    }

    // Link the Google login to the (new or existing) user
    var addLoginResult = await userManager.AddLoginAsync(user, info);
    if (!addLoginResult.Succeeded)
    {
      var errors = string.Join(" ", addLoginResult.Errors.Select(e => e.Description));
      await Send.ResultAsync(Results.Redirect(
        loginUrl + "&error=" + Uri.EscapeDataString(errors)));
      return;
    }

    await signInManager.SignInAsync(user, isPersistent: false);
    await Send.ResultAsync(Results.Redirect(returnUrl));
  }
}

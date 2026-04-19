using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Identity.Entities;

namespace Identity.Features.Account.Login.External;

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
        loginUrl + "&error=" + Uri.EscapeDataString("External sign-in failed. Please try again.")));
      return;
    }

    var email = info.Principal.FindFirstValue(ClaimTypes.Email);
    if (string.IsNullOrWhiteSpace(email))
    {
      await Send.ResultAsync(Results.Redirect(
        loginUrl + "&error=" + Uri.EscapeDataString("No email address was returned by the provider.")));
      return;
    }

    var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName) ?? email;
    var lastName = info.Principal.FindFirstValue(ClaimTypes.Surname) ?? email;
    var picture = info.Principal.FindFirstValue("picture");

    // Try sign-in with an existing linked external login
    var signInResult = await signInManager.ExternalLoginSignInAsync(
      info.LoginProvider, info.ProviderKey,
      isPersistent: false, bypassTwoFactor: true);

    if (signInResult.Succeeded)
    {
      var user = await userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
      if (user is not null)
        await SyncProfileAsync(user, firstName, lastName, picture);

      await Send.ResultAsync(Results.Redirect(returnUrl));
      return;
    }

    // No linked login yet – find or create the local user
    var localUser = await userManager.FindByEmailAsync(email);

    if (localUser is null)
    {
      localUser = new User
      {
        UserName = email,
        Email = email,
        EmailConfirmed = true,
        FirstName = firstName,
        LastName = lastName,
        Picture = picture
      };

      var createResult = await userManager.CreateAsync(localUser);
      if (!createResult.Succeeded)
      {
        var errors = string.Join(" ", createResult.Errors.Select(e => e.Description));
        await Send.ResultAsync(Results.Redirect(
          loginUrl + "&error=" + Uri.EscapeDataString(errors)));
        return;
      }
    }
    else
    {
      await SyncProfileAsync(localUser, firstName, lastName, picture);
    }

    var addLoginResult = await userManager.AddLoginAsync(localUser, info);
    if (!addLoginResult.Succeeded)
    {
      var errors = string.Join(" ", addLoginResult.Errors.Select(e => e.Description));
      await Send.ResultAsync(Results.Redirect(
        loginUrl + "&error=" + Uri.EscapeDataString(errors)));
      return;
    }

    await signInManager.SignInAsync(localUser, isPersistent: false);
    await Send.ResultAsync(Results.Redirect(returnUrl));
  }

  private async Task SyncProfileAsync(User user, string firstName, string lastName, string? picture)
  {
    var changed =
      user.FirstName != firstName ||
      user.LastName != lastName ||
      user.Picture != picture;

    if (!changed) return;

    user.FirstName = firstName;
    user.LastName = lastName;
    user.Picture = picture;

    await userManager.UpdateAsync(user);
  }
}

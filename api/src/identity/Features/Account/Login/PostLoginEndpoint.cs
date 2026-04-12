using Microsoft.AspNetCore.Identity;
using Identity.Entities;

namespace Identity.Features.Account.Login;

public class PostLoginEndpoint(SignInManager<User> signInManager) : EndpointWithoutRequest<bool>
{
  public override void Configure()
  {
    Post("/account/login");
    AllowAnonymous();
    Description(x => x.WithName("PostLogin"));
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var form = await HttpContext.Request.ReadFormAsync(ct);
    var username = form["username"].ToString();
    var password = form["password"].ToString();
    var returnUrl = form["returnUrl"].ToString();
    if (string.IsNullOrEmpty(returnUrl)) returnUrl = "/";

    var result = await signInManager.PasswordSignInAsync(
      username, password, isPersistent: false, lockoutOnFailure: false);

    if (!result.Succeeded)
    {
      var redirectPath = $"/login?returnUrl={Uri.EscapeDataString(returnUrl)}" +
                         $"&error={Uri.EscapeDataString("Invalid username or password.")}";
      await Send.ResultAsync(Results.Redirect(redirectPath));
      return;
    }

    await Send.ResultAsync(Results.Redirect(returnUrl));
  }
}

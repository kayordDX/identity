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
      await Send.ResultAsync(
        Results.Content(LoginPageHelper.LoginPageHtml(returnUrl, "Invalid username or password."), "text/html"));
      return;
    }

    await Send.ResultAsync(Results.Redirect(returnUrl));
  }
}

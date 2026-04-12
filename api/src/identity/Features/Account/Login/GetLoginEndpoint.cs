namespace Identity.Features.Account.Login;

public class GetLoginEndpoint : EndpointWithoutRequest
{
  public override void Configure()
  {
    Get("/account/login");
    AllowAnonymous();
    Description(x => x.WithName("GetLogin"));
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var returnUrl = HttpContext.Request.Query["ReturnUrl"].FirstOrDefault() ?? "/";
    var error = HttpContext.Request.Query["error"].FirstOrDefault();

    var redirectPath = $"/login?returnUrl={Uri.EscapeDataString(returnUrl)}";
    if (!string.IsNullOrEmpty(error))
      redirectPath += $"&error={Uri.EscapeDataString(error)}";

    await Send.ResultAsync(Results.Redirect(redirectPath));
  }
}

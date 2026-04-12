namespace Identity.Features.Account.Login;

public class GetLoginEndpoint : EndpointWithoutRequest<bool>
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
    await Send.ResultAsync(Results.Content(LoginPageHelper.LoginPageHtml(returnUrl), "text/html"));
  }
}

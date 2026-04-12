namespace Identity.Features.Connect.Authorize;

public class GetAuthorizeEndpoint : EndpointWithoutRequest<bool>
{
  public override void Configure()
  {
    Get("connect/authorize");
    AllowAnonymous();
    Description(x => x.WithName("GetAuthorize"));
  }

  public override Task HandleAsync(CancellationToken ct)
    => AuthorizeLogic.HandleAsync(HttpContext, r => Send.ResultAsync(r));
}

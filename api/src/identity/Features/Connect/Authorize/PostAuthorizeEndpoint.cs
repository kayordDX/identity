namespace Identity.Features.Connect.Authorize;

public class PostAuthorizeEndpoint : EndpointWithoutRequest<bool>
{
  public override void Configure()
  {
    Post("connect/authorize");
    AllowAnonymous();
    Description(x => x.WithName("PostAuthorize"));
  }

  public override Task HandleAsync(CancellationToken ct)
    => AuthorizeLogic.HandleAsync(HttpContext, r => Send.ResultAsync(r));
}

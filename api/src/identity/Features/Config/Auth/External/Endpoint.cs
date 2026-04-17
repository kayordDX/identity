namespace Identity.Features.Config.Auth.External;

public class GetGoogleClientIdEndpoint(IConfiguration config) : EndpointWithoutRequest<ConfigAuthExternalResponse>
{
  public override void Configure()
  {
    Get("/config/auth/external");
    AllowAnonymous();
    Description(x => x.WithName("ConfigAuthExternal"));
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var googleClientId = config["Authentication:Google:ClientId"];

    var result = new ConfigAuthExternalResponse
    {
      GoogleClientId = googleClientId
    };
    await Send.OkAsync(result, ct);
  }
}

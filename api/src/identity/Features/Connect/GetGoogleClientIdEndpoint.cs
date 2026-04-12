namespace Identity.Features.Connect;

public class GoogleClientIdResponse
{
  public string? ClientId { get; set; }
}

public class GetGoogleClientIdEndpoint(IConfiguration config) : EndpointWithoutRequest<GoogleClientIdResponse>
{
  public override void Configure()
  {
    Get("/api/config/google-client-id");
    AllowAnonymous();
    Description(x => x.WithName("GetGoogleClientId"));
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var clientId = config["Authentication:Google:ClientId"];

    await Send.OkAsync(new GoogleClientIdResponse
    {
      ClientId = clientId
    }, ct);
  }
}

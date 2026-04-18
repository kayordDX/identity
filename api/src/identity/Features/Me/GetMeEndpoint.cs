using System.Security.Claims;
using OidcClaims = OpenIddict.Abstractions.OpenIddictConstants.Claims;

namespace Identity.Features.Me;

public class GetMeEndpoint : EndpointWithoutRequest<MeResponse>
{
  public override void Configure()
  {
    Get("/me");
    Description(x => x.WithName("GetMe"));
  }

  public override Task HandleAsync(CancellationToken ct)
    => Send.OkAsync(new MeResponse
    {
      Sub = HttpContext.User.FindFirstValue(OidcClaims.Subject),
      Name = HttpContext.User.FindFirstValue(OidcClaims.Name),
      Email = HttpContext.User.FindFirstValue(OidcClaims.Email),
    }, ct);
}

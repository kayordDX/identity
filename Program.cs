using System.Security.Claims;
using Identity.Data;
using Identity.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Quartz;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddQuartz(options =>
{
  options.UseSimpleTypeLoader();
  options.UseInMemoryStore();
});

builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

builder.Services.AddDbContext<AppDbContext>(options =>
{
  // options.UseSnakeCaseNamingConvention();
  // options.UseNpgsql(
  //     builder.Configuration.GetConnectionString("DefaultConnection")
  // // b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
  // );
  options.UseSqlite($"Filename={Path.Combine(Path.GetTempPath(), "db.sqlite3")}");
  options.UseOpenIddict();
});

builder.Services.AddIdentity<User, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();


builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
      options.UseEntityFrameworkCore().UseDbContext<AppDbContext>();
    })
    .AddClient(options =>
    {
      options.AllowAuthorizationCodeFlow();
      options.AddDevelopmentEncryptionCertificate()
             .AddDevelopmentSigningCertificate();
      options.UseAspNetCore()
             .EnableRedirectionEndpointPassthrough();
      options.UseSystemNetHttp()
             .SetProductInformation(typeof(Program).Assembly);
      options.UseWebProviders()
             .AddGitHub(options =>
             {
               options.SetClientId("c4ade52327b01ddacff3")
                        .SetClientSecret("da6bed851b75e317bf6b2cb67013679d9467c122")
                        .SetRedirectUri("callback/login/github");
             });
    })
    .AddServer(options =>
    {
      options
        .SetAuthorizationEndpointUris("connect/authorize")
        .SetIntrospectionEndpointUris("connect/introspect")
        .SetTokenEndpointUris("connect/token");
      options
        .AllowAuthorizationCodeFlow()
        .AllowRefreshTokenFlow();
      options.AddDevelopmentEncryptionCertificate()
             .AddDevelopmentSigningCertificate();
      options.UseAspNetCore()
             .EnableAuthorizationEndpointPassthrough();
    })
    .AddValidation(options =>
    {
      options.UseLocalServer();
      options.UseAspNetCore();
    });


builder.Services.AddAuthorization()
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("api", (ClaimsPrincipal user) => user.Identity!.Name).RequireAuthorization();

app.MapMethods("callback/login/github", [HttpMethods.Get, HttpMethods.Post], async (HttpContext context) =>
{
  // Resolve the claims extracted by OpenIddict from the userinfo response returned by GitHub.
  var result = await context.AuthenticateAsync(Providers.GitHub);

  var identity = new ClaimsIdentity(
      authenticationType: "ExternalLogin",
      nameType: ClaimTypes.Name,
      roleType: ClaimTypes.Role);

  identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, result.Principal!.FindFirst("id")!.Value));

  var properties = new AuthenticationProperties
  {
    RedirectUri = result.Properties!.RedirectUri
  };

  // For scenarios where the default sign-in handler configured in the ASP.NET Core
  // authentication options shouldn't be used, a specific scheme can be specified here.
  return Results.SignIn(new ClaimsPrincipal(identity), properties);
});

app.MapMethods("connect/authorize", [HttpMethods.Get, HttpMethods.Post], async (HttpContext context) =>
{
  // Resolve the claims stored in the cookie created after the GitHub authentication dance.
  // If the principal cannot be found, trigger a new challenge to redirect the user to GitHub.
  //
  // For scenarios where the default authentication handler configured in the ASP.NET Core
  // authentication options shouldn't be used, a specific scheme can be specified here.
  var principal = (await context.AuthenticateAsync())?.Principal;
  if (principal is not { Identity.IsAuthenticated: true })
  {
    var properties = new AuthenticationProperties
    {
      RedirectUri = context.Request.GetEncodedUrl()
    };

    return Results.Challenge(properties, [Providers.GitHub]);
  }

  var identifier = principal.FindFirst(ClaimTypes.NameIdentifier)!.Value;

  // Create the claims-based identity that will be used by OpenIddict to generate tokens.
  var identity = new ClaimsIdentity(
      authenticationType: TokenValidationParameters.DefaultAuthenticationType,
      nameType: Claims.Name,
      roleType: Claims.Role);

  // Import a few select claims from the identity stored in the local cookie.
  identity.AddClaim(new Claim(Claims.Subject, identifier));
  identity.AddClaim(new Claim(Claims.Name, identifier).SetDestinations(Destinations.AccessToken));
  identity.AddClaim(new Claim(Claims.PreferredUsername, identifier).SetDestinations(Destinations.AccessToken));

  return Results.SignIn(new ClaimsPrincipal(identity), properties: null, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
});

await using (var scope = app.Services.CreateAsyncScope())
{
  var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
  await context.Database.EnsureCreatedAsync();

  var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

  if (await manager.FindByClientIdAsync("console_app") == null)
  {
    await manager.CreateAsync(new OpenIddictApplicationDescriptor
    {
      ApplicationType = ApplicationTypes.Native,
      ClientId = "console_app",
      ClientType = ClientTypes.Public,
      RedirectUris =
            {
                new Uri("http://localhost/")
            },
      Permissions =
            {
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.Token,
                Permissions.GrantTypes.AuthorizationCode,
                Permissions.ResponseTypes.Code
            }
    });
  }
}
app.Run();

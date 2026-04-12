using System.Security.Claims;
using Microsoft.AspNetCore;
using Identity.Data;
using Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using Quartz;
using static OpenIddict.Abstractions.OpenIddictConstants;
using Identity.Common.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureApi();

builder.Services.AddQuartz(options =>
{
  options.UseSimpleTypeLoader();
  options.UseInMemoryStore();
});
builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

builder.Services.AddDbContext<AppDbContext>(options =>
{
  options.UseSqlite("Filename=identity.db");
  options.UseOpenIddict();
});

builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
  {
    // Relax password rules for development
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
  })
  .AddEntityFrameworkStores<AppDbContext>()
  .AddDefaultTokenProviders();

// Point Identity's cookie login path to our FastEndpoints handler
builder.Services.ConfigureApplicationCookie(options =>
{
  options.LoginPath = "/account/login";
});

// ── OpenIddict ──────────────────────────────────────────────────────────────
builder.Services.AddOpenIddict()
  .AddCore(options =>
  {
    options.UseEntityFrameworkCore().UseDbContext<AppDbContext>();
    options.UseQuartz();
  })
  .AddServer(options =>
  {
    options
      .SetAuthorizationEndpointUris("connect/authorize")
      .SetTokenEndpointUris("connect/token")
      .SetUserInfoEndpointUris("connect/userinfo")
      .SetEndSessionEndpointUris("connect/logout");

    options
      .AllowAuthorizationCodeFlow().RequireProofKeyForCodeExchange()
      .AllowRefreshTokenFlow();

    options.RegisterScopes(
      Scopes.OpenId,
      Scopes.Profile,
      Scopes.Email,
      Scopes.OfflineAccess);

    options
      .AddDevelopmentEncryptionCertificate()
      .AddDevelopmentSigningCertificate();

    options.UseAspNetCore()
      .DisableTransportSecurityRequirement() // Allow HTTP in development
      .EnableAuthorizationEndpointPassthrough()
      .EnableEndSessionEndpointPassthrough()
      .EnableUserInfoEndpointPassthrough()
      .EnableStatusCodePagesIntegration();
  })
  .AddValidation(options =>
  {
    options.UseLocalServer();
    options.UseAspNetCore();
  });

// ── Authorization ───────────────────────────────────────────────────────────
builder.Services.AddAuthorization(options =>
{
  // Used for bearer-token-protected API endpoints
  options.AddPolicy("ApiPolicy", policy =>
    policy
      .AddAuthenticationSchemes(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)
      .RequireAuthenticatedUser());
});

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseApi();

// ── Seed database ───────────────────────────────────────────────────────────
await SeedAsync(app.Services);

app.Run();

// ─── Startup helpers ────────────────────────────────────────────────────────

static async Task SeedAsync(IServiceProvider services)
{
  await using var scope = services.CreateAsyncScope();

  var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
  await db.Database.EnsureCreatedAsync();

  // ── Register the web test client ──
  var appManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

  if (await appManager.FindByClientIdAsync("web_client") is null)
  {
    await appManager.CreateAsync(new OpenIddictApplicationDescriptor
    {
      ClientId = "web_client",
      ClientType = ClientTypes.Public,
      DisplayName = "Web Test Client",
      RedirectUris =
      {
        new Uri("https://localhost:7199/"),
        new Uri("http://localhost:5214/"),
      },
      PostLogoutRedirectUris =
      {
        new Uri("https://localhost:7199/"),
        new Uri("http://localhost:5214/"),
      },
      Permissions =
      {
        Permissions.Endpoints.Authorization,
        Permissions.Endpoints.Token,
        Permissions.Endpoints.EndSession,
        Permissions.GrantTypes.AuthorizationCode,
        Permissions.GrantTypes.RefreshToken,
        Permissions.ResponseTypes.Code,
        Permissions.Scopes.Profile,
        Permissions.Scopes.Email,
      },
    });
  }

  // ── Seed a default admin user ──
  var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

  if (await userManager.FindByNameAsync("admin") is null)
  {
    var admin = new User
    {
      UserName = "admin",
      Email = "admin@example.com",
      DisplayName = "Admin"
    };
    await userManager.CreateAsync(admin, "password");
  }
}

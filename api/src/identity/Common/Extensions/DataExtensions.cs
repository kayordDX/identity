using Identity.Data;
using Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Identity.Common.Extensions;

public static class DataExtensions
{

  public static IServiceCollection ConfigureEF(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
  {
    services.AddDbContext<AppDbContext>(options =>
    {
      options.UseSnakeCaseNamingConvention();
      options.UseNpgsql(
          configuration.GetConnectionString("DefaultConnection"),
          b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
      );
      options.UseOpenIddict();
      if (env.IsDevelopment())
      {
        options.EnableSensitiveDataLogging();
      }
    });

    services.AddIdentity<User, Role>(options =>
      {
        // Relax password rules for development
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
        options.User.RequireUniqueEmail = true;
      })
      .AddEntityFrameworkStores<AppDbContext>()
      .AddDefaultTokenProviders();

    return services;
  }


  public static async Task ApplyMigrations(this IServiceProvider serviceProvider, CancellationToken ct)
  {
    await using var scope = serviceProvider.CreateAsyncScope();

    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync(ct);

    var appManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

    var descriptor = new OpenIddictApplicationDescriptor
    {
      ClientId = "web_client",
      // ClientSecret = "web_client_secret",
      ClientType = ClientTypes.Public,
      DisplayName = "Web Test Client",
      RedirectUris =
      {
        new Uri("https://localhost:7199/"),
        new Uri("http://localhost:5214/"),
        new Uri("http://localhost:5173/"),          // Svelte dev server
        new Uri("http://localhost:5000/auth/login/callback"),
        new Uri("http://localhost:5000/signin-oidc"),
      },
      PostLogoutRedirectUris =
      {
        new Uri("https://localhost:7199/"),
        new Uri("http://localhost:5214/"),
        new Uri("http://localhost:5173/"),          // Svelte dev server
        new Uri("http://localhost:5173/test"),          // Svelte dev server
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
    };

    var existingClient = await appManager.FindByClientIdAsync("web_client", ct);
    if (existingClient is null)
    {
      await appManager.CreateAsync(descriptor, ct);
    }
    else
    {
      await appManager.PopulateAsync(existingClient, descriptor, ct);
      await appManager.UpdateAsync(existingClient, ct);
    }

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

}

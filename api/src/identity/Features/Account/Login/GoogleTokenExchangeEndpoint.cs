using System.Security.Claims;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Identity.Entities;

namespace Identity.Features.Account.Login;

public class GoogleTokenExchangeResponse
{
  public bool Success { get; set; }
  public string UserId { get; set; } = string.Empty;
  public string Email { get; set; } = string.Empty;
  public string DisplayName { get; set; } = string.Empty;
  public string Message { get; set; } = string.Empty;
}

public class GoogleTokenExchangeEndpoint(
  UserManager<User> userManager,
  SignInManager<User> signInManager) : Endpoint<GoogleTokenExchangeRequest, GoogleTokenExchangeResponse>
{
  public override void Configure()
  {
    Post("/account/login/google/exchange");
    AllowAnonymous();
    Description(x => x.WithName("ExchangeGoogleToken"));
  }

  public override async Task HandleAsync(GoogleTokenExchangeRequest req, CancellationToken ct)
  {
    if (string.IsNullOrWhiteSpace(req.GoogleIdToken))
    {
      await Send.ResultAsync(Results.BadRequest(new GoogleTokenExchangeResponse
      {
        Success = false,
        Message = "google_id_token is required"
      }));
      return;
    }

    // Validate Google ID token using Google's public keys
    GoogleJsonWebSignature.Payload payload;
    try
    {
      payload = await GoogleJsonWebSignature.ValidateAsync(req.GoogleIdToken);
    }
    catch (Exception ex)
    {
      await Send.ResultAsync(Results.BadRequest(new GoogleTokenExchangeResponse
      {
        Success = false,
        Message = $"Invalid or expired Google ID token: {ex.Message}"
      }));
      return;
    }

    if (!payload.EmailVerified)
    {
      await Send.ResultAsync(Results.BadRequest(new GoogleTokenExchangeResponse
      {
        Success = false,
        Message = "Google account email is not verified"
      }));
      return;
    }

    var email = payload.Email;
    if (string.IsNullOrWhiteSpace(email))
    {
      await Send.ResultAsync(Results.BadRequest(new GoogleTokenExchangeResponse
      {
        Success = false,
        Message = "No email address in Google token"
      }));
      return;
    }

    // Find or auto-provision local user
    var user = await userManager.FindByEmailAsync(email);
    if (user is null)
    {
      user = new User
      {
        UserName = email,
        Email = email,
        EmailConfirmed = true,
        DisplayName = payload.Name ?? email,
      };

      var createResult = await userManager.CreateAsync(user);
      if (!createResult.Succeeded)
      {
        var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
        await Send.ResultAsync(Results.BadRequest(new GoogleTokenExchangeResponse
        {
          Success = false,
          Message = $"Failed to create user: {errors}"
        }));
        return;
      }
    }

    // Link Google login if not already linked
    var existingGoogleLogin = await userManager.FindByLoginAsync("Google", payload.Subject);
    if (existingGoogleLogin?.Id != user.Id)
    {
      var addLoginResult = await userManager.AddLoginAsync(
        user,
        new UserLoginInfo("Google", payload.Subject, "Google"));

      if (!addLoginResult.Succeeded && !addLoginResult.Errors.Any(e => e.Code == "LoginAlreadyAssociated"))
      {
        var errors = string.Join(", ", addLoginResult.Errors.Select(e => e.Description));
        await Send.ResultAsync(Results.BadRequest(new GoogleTokenExchangeResponse
        {
          Success = false,
          Message = $"Failed to link Google account: {errors}"
        }));
        return;
      }
    }

    // Sign in the user via the identity server
    await signInManager.SignInAsync(user, isPersistent: false);

    await Send.OkAsync(new GoogleTokenExchangeResponse
    {
      Success = true,
      UserId = user.Id.ToString(),
      Email = user.Email ?? "",
      DisplayName = user.DisplayName ?? user.UserName ?? "",
      Message = "Successfully authenticated with Google"
    }, ct);
  }
}

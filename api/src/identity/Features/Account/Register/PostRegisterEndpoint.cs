using Identity.Entities;
using Microsoft.AspNetCore.Identity;

namespace Identity.Features.Account.Register;

public class PostRegisterEndpoint(UserManager<User> userManager) : Endpoint<RegisterRequest, RegisterResponse>
{
  public override void Configure()
  {
    Post("/account/register");
    AllowAnonymous();
    Description(x => x.WithName("PostRegister"));
  }

  public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
  {
    if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
    {
      await Send.ResultAsync(Results.BadRequest(new { error = "Username and password are required." }));
      return;
    }

    var user = new User
    {
      Email = req.Email,
      UserName = req.Email,
      FirstName = req.FirstName,
      LastName = req.LastName
    };

    var result = await userManager.CreateAsync(user, req.Password);

    if (!result.Succeeded)
    {
      await Send.ResultAsync(Results.BadRequest(new
      {
        error = string.Join("; ", result.Errors.Select(e => e.Description))
      }));
      return;
    }

    await Send.OkAsync(new RegisterResponse { Message = "User registered successfully." }, ct);
  }
}

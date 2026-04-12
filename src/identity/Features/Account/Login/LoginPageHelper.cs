using System.Net;

namespace Identity.Features.Account.Login;

internal static class LoginPageHelper
{
  internal static string LoginPageHtml(string returnUrl, string? error = null)
  {
    var encodedReturnUrl = WebUtility.HtmlEncode(returnUrl);
    var errorHtml = error is not null
      ? $"""<p class="error">{WebUtility.HtmlEncode(error)}</p>"""
      : string.Empty;

    return $$"""
      <!DOCTYPE html>
      <html lang="en">
      <head>
        <meta charset="utf-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1" />
        <title>Sign In – Identity Server</title>
        <style>
          *, *::before, *::after { box-sizing: border-box; }
          body { font-family: system-ui, sans-serif; background: #f3f4f6;
                 display: flex; align-items: center; justify-content: center;
                 min-height: 100vh; margin: 0; }
          .card { background: #fff; padding: 2rem; border-radius: 12px;
                  box-shadow: 0 4px 24px rgba(0,0,0,.08);
                  width: 100%; max-width: 380px; }
          h1 { margin: 0 0 1.5rem; font-size: 1.5rem; text-align: center; }
          label { display: block; font-size: .875rem; font-weight: 500;
                  margin-bottom: .25rem; }
          input { display: block; width: 100%; padding: .625rem .75rem;
                  border: 1px solid #d1d5db; border-radius: 6px;
                  font-size: 1rem; margin-bottom: 1rem; }
          button { display: block; width: 100%; padding: .65rem;
                   background: #4f46e5; color: #fff; border: none;
                   border-radius: 6px; font-size: 1rem; cursor: pointer; }
          button:hover { background: #4338ca; }
          .error { color: #dc2626; font-size: .875rem; margin-bottom: 1rem;
                   padding: .5rem .75rem; background: #fef2f2;
                   border-radius: 6px; }
        </style>
      </head>
      <body>
        <div class="card">
          <h1>🔐 Sign In</h1>
          {{errorHtml}}
          <form method="post" action="/account/login">
            <input type="hidden" name="returnUrl" value="{{encodedReturnUrl}}" />
            <label for="username">Username</label>
            <input id="username" type="text" name="username"
                   required autofocus autocomplete="username" />
            <label for="password">Password</label>
            <input id="password" type="password" name="password"
                   required autocomplete="current-password" />
            <button type="submit">Sign In</button>
          </form>
        </div>
      </body>
      </html>
      """;
  }
}

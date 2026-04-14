using Identity.Common.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureApi();
builder.Services.ConfigureQuartz();
builder.Services.ConfigureEF(builder.Configuration, builder.Environment);

builder.Services.ConfigureGoogleAuth(builder.Configuration);
builder.Services.ConfigureOpenIddict(builder.Configuration);
builder.Services.ConfigureAuth();
builder.Services.ConfigureCors(builder.Configuration);

var app = builder.Build();

await app.Services.ApplyMigrations(app.Lifetime.ApplicationStopping);

app.UseCorsIdentity();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseApi();
app.MapFallbackToFile("index.html");

app.Run();

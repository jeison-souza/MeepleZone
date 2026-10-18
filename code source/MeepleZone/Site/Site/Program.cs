using MeepleZone.Site.Authentification;
using MeepleZone.Site.Components;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMudServices();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services
    .AddAuthorization()
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie()
    .AddGoogle(options =>
    {
        builder.Configuration.GetSection("Authentication:Google").Bind(options);
        options.Events.OnRemoteFailure = GestionEchecConnexion.Traiter;
        options.Events.OnAccessDenied = GestionEchecConnexion.TraiterRefus;
    })
    .AddMicrosoftAccount(options =>
    {
        builder.Configuration.GetSection("Authentication:Microsoft").Bind(options);
        options.Events.OnRemoteFailure = GestionEchecConnexion.Traiter;
        options.Events.OnAccessDenied = GestionEchecConnexion.TraiterRefus;
    });

builder.Services.AddOptions<GoogleOptions>(GoogleDefaults.AuthenticationScheme)
    .Validate(o => !string.IsNullOrWhiteSpace(o.ClientId) && !string.IsNullOrWhiteSpace(o.ClientSecret),
              "Authentication:Google : ClientId ou ClientSecret manquant.")
    .ValidateOnStart();

builder.Services.AddOptions<MicrosoftAccountOptions>(MicrosoftAccountDefaults.AuthenticationScheme)
    .Validate(o => !string.IsNullOrWhiteSpace(o.ClientId) && !string.IsNullOrWhiteSpace(o.ClientSecret),
              "Authentication:Microsoft : ClientId ou ClientSecret manquant.")
    .ValidateOnStart();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/erreur", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/introuvable", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication()
   .UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(MeepleZone.Site.Client._Imports).Assembly);

// (Endpoints)
app.MapGet("/connexion/Google", () => Results.Challenge(new AuthenticationProperties { RedirectUri = "/" }, [GoogleDefaults.AuthenticationScheme]));
app.MapGet("/connexion/Microsoft", () => Results.Challenge(new AuthenticationProperties { RedirectUri = "/" }, [MicrosoftAccountDefaults.AuthenticationScheme]));
app.MapPost("/deconnexion", () => Results.SignOut(new AuthenticationProperties { RedirectUri = "/" }, [CookieAuthenticationDefaults.AuthenticationScheme]));

app.Run();

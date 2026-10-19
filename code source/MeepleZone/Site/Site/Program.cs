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

// Fournit l'état d'authentification aux composants (AuthorizeView, AuthorizeRouteView).
builder.Services.AddCascadingAuthenticationState();

builder.Services
    .AddAuthorization()
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Une page protégée ouverte sans connexion mène à la page de connexion, avec l'adresse d'origine.
        options.LoginPath = "/connexion-requise";
        options.ReturnUrlParameter = "retour";
    })
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
// L'adresse de retour passe toujours par UrlRetour : seul un chemin local du site est accepté.
app.MapGet("/connexion/Google", (string? retour) => Results.Challenge(new AuthenticationProperties { RedirectUri = UrlRetour.Valider(retour) }, [GoogleDefaults.AuthenticationScheme]));
app.MapGet("/connexion/Microsoft", (string? retour) => Results.Challenge(new AuthenticationProperties { RedirectUri = UrlRetour.Valider(retour) }, [MicrosoftAccountDefaults.AuthenticationScheme]));

// Le paramètre de formulaire fait valider le jeton antiforgery par l'intergiciel : sans jeton, la requête est refusée (400).
app.MapPost("/deconnexion", (IFormCollection _) => Results.SignOut(new AuthenticationProperties { RedirectUri = "/" }, [CookieAuthenticationDefaults.AuthenticationScheme]));

app.Run();

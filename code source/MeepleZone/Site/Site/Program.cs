using MeepleZone.Domaines.Compte.Extensions;
using MeepleZone.Site.Authentification;
using MeepleZone.Site.Authentification.Extensions;
using MeepleZone.Site.Components;
using MeepleZone.Systeme.Donnees.Extensions;
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
using System;

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
        options.Events.OnCreatingTicket = GestionConnexionCompte.Traiter;
    })
    .AddMicrosoftAccount(options =>
    {
        builder.Configuration.GetSection("Authentication:Microsoft").Bind(options);
        options.ConfigurerRevendications();
        options.Events.OnRemoteFailure = GestionEchecConnexion.Traiter;
        options.Events.OnAccessDenied = GestionEchecConnexion.TraiterRefus;
        options.Events.OnCreatingTicket = GestionConnexionCompte.Traiter;
    });

builder.Services.AddOptions<GoogleOptions>(GoogleDefaults.AuthenticationScheme)
    .Validate(o => !string.IsNullOrWhiteSpace(o.ClientId) && !string.IsNullOrWhiteSpace(o.ClientSecret),
              "Authentication:Google : ClientId ou ClientSecret manquant.")
    .ValidateOnStart();

builder.Services.AddOptions<MicrosoftAccountOptions>(MicrosoftAccountDefaults.AuthenticationScheme)
    .Validate(o => !string.IsNullOrWhiteSpace(o.ClientId) && !string.IsNullOrWhiteSpace(o.ClientSecret),
              "Authentication:Microsoft : ClientId ou ClientSecret manquant.")
    .ValidateOnStart();

// À activer quand le domaine Collection sera implémenté (la page « Ma collection » en dépend) :
// ajouter .AjouterDomaineCollection() à la chaîne ci-dessous.
builder.Services
    .AjouterSystemeDonnees(builder.Configuration.GetConnectionString("MeepleZone") ?? throw new InvalidOperationException("ConnectionStrings:MeepleZone manquant."))
    .AjouterDomaineCompte();

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
app.MapGet("/connexion/Google", (string? retour, string? creation) => Results.Challenge(ProprietesConnexion.Creer(retour, creation), [GoogleDefaults.AuthenticationScheme]));
app.MapGet("/connexion/Microsoft", (string? retour, string? creation) => Results.Challenge(ProprietesConnexion.Creer(retour, creation), [MicrosoftAccountDefaults.AuthenticationScheme]));

// Le paramètre de formulaire fait valider le jeton antiforgery par l'intergiciel : sans jeton, la requête est refusée (400).
app.MapPost("/deconnexion", (IFormCollection _) => Results.SignOut(new AuthenticationProperties { RedirectUri = "/" }, [CookieAuthenticationDefaults.AuthenticationScheme]));

app.Run();

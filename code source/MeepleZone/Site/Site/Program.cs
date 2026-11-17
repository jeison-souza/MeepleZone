using MeepleZone.Domaines.Collection.Extensions;
using MeepleZone.Domaines.Compte;
using MeepleZone.Domaines.Compte.Extensions;
using MeepleZone.Site.Authentification;
using MeepleZone.Site.Authentification.Extensions;
using MeepleZone.Site.Authentification.Liaison;
using MeepleZone.Site.Authentification.Suppression;
using MeepleZone.Site.Components;
using MeepleZone.Site.Extensions;
using MeepleZone.Systeme.Donnees.Extensions;
using MeepleZone.Systeme.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMudServices();

// Production : proxy inverse (en-têtes transmis) et clés de protection des données conservées entre les redémarrages.
builder.Services
    .ConfigurerEnTetesTransmis(builder.Configuration)
    .ConfigurerProtectionDonnees(builder.Configuration);

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
        options.Events.OnTicketReceived = GestionLiaisonCompte.Traiter;
    })
    .AddMicrosoftAccount(options =>
    {
        builder.Configuration.GetSection("Authentication:Microsoft").Bind(options);
        options.ConfigurerRevendications();
        options.Events.OnRemoteFailure = GestionEchecConnexion.Traiter;
        options.Events.OnAccessDenied = GestionEchecConnexion.TraiterRefus;
        options.Events.OnCreatingTicket = GestionConnexionCompte.Traiter;
        options.Events.OnTicketReceived = GestionLiaisonCompte.Traiter;
    });

builder.Services.AddOptions<GoogleOptions>(GoogleDefaults.AuthenticationScheme)
    .Validate(o => !string.IsNullOrWhiteSpace(o.ClientId) && !string.IsNullOrWhiteSpace(o.ClientSecret),
              "Authentication:Google : ClientId ou ClientSecret manquant.")
    .ValidateOnStart();

builder.Services.AddOptions<MicrosoftAccountOptions>(MicrosoftAccountDefaults.AuthenticationScheme)
    .Validate(o => !string.IsNullOrWhiteSpace(o.ClientId) && !string.IsNullOrWhiteSpace(o.ClientSecret),
              "Authentication:Microsoft : ClientId ou ClientSecret manquant.")
    .ValidateOnStart();

// Identité de la session, requise par les gestionnaires des domaines Collection et Compte.
builder.Services.AddScoped<IUtilisateurCourant, UtilisateurCourant>();

// IUtilisateurCourant lit l'utilisateur de la requête quand il n'y a pas de circuit Blazor (retour du fournisseur OAuth).
builder.Services.AddHttpContextAccessor();

builder.Services
    .AjouterSystemeDonnees(builder.Configuration.GetConnectionString("MeepleZone") ?? throw new InvalidOperationException("ConnectionStrings:MeepleZone manquant."))
    .AjouterDomaineCompte()
    .AjouterDomaineCollection();

var app = builder.Build();

// Doit venir en premier : le schéma (https) et l'adresse doivent être corrigés avant HSTS, la redirection HTTPS et l'authentification.
app.UseForwardedHeaders();

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

// Liaison d'un fournisseur à un compte déjà connecté (sans session, le cookie mène à /connexion-requise). Un fournisseur hors de la liste donne 404.
app.MapGet("/liaison/{fournisseur}", (string fournisseur) =>
        FournisseursConnexion.Trouver(fournisseur) is { } connu
            ? Results.Challenge(GestionLiaisonCompte.CreerProprietes(), [connu.Nom])
            : Results.NotFound())
    .RequireAuthorization();

// Suppression du compte : le paramètre de formulaire fait valider le jeton antiforgery; le code d'amitié tapé est revérifié ici.
app.MapPost("/compte/suppression", (IFormCollection formulaire, IDomaineCompte domaine, ILoggerFactory fabrique) =>
        GestionSuppressionCompte.Traiter(
            formulaire[GestionSuppressionCompte.ChampConfirmation].ToString(),
            domaine,
            fabrique.CreateLogger(typeof(GestionSuppressionCompte))))
    .RequireAuthorization();

// Le paramètre de formulaire fait valider le jeton antiforgery par l'intergiciel : sans jeton, la requête est refusée (400).
app.MapPost("/deconnexion", (IFormCollection _) => Results.SignOut(new AuthenticationProperties { RedirectUri = "/" }, [CookieAuthenticationDefaults.AuthenticationScheme]));

app.Run();

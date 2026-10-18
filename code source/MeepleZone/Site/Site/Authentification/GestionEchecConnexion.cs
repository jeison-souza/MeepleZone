using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeepleZone.Site.Authentification;

/// <summary>
/// Traite l'échec du retour d'un fournisseur d'identité externe (Google, Microsoft) :
/// redirige vers la page de connexion au lieu de laisser l'exception produire une erreur 500.
/// </summary>
public static class GestionEchecConnexion
{
    // Codes renvoyés par les fournisseurs quand la personne refuse ou annule le consentement.
    private static readonly string[] CodesAnnulation = ["access_denied", "consent_required"];

    public static Task Traiter(RemoteFailureContext context)
    {
        var annulee = context.Failure is AuthenticationFailureException echec
            && CodesAnnulation.Any(code => echec.Message.StartsWith(code, StringComparison.Ordinal));

        // Une annulation est un choix de la personne; tout le reste est une vraie erreur à consigner.
        if (!annulee)
        {
            context.HttpContext.RequestServices
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger(typeof(GestionEchecConnexion))
                .LogWarning(context.Failure, "Échec de la connexion externe ({Fournisseur}).", context.Scheme.Name);
        }

        // Le motif ne contient aucune donnée du fournisseur ni de la personne.
        context.Response.Redirect(annulee ? "/connexion-requise?motif=annulee" : "/connexion-requise?motif=erreur");

        // Sans cet appel, l'exception serait quand même levée après la redirection.
        context.HandleResponse();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Refus explicite (<c>error=access_denied</c>), par exemple « Annuler » chez Google. ASP.NET Core ne passe pas
    /// par <see cref="Traiter"/> dans ce cas : il lève une exception générique, sans le code du fournisseur,
    /// et appelle plutôt cet événement dédié.
    /// </summary>
    public static Task TraiterRefus(AccessDeniedContext context)
    {
        context.Response.Redirect("/connexion-requise?motif=annulee");
        context.HandleResponse();
        return Task.CompletedTask;
    }
}

using MeepleZone.Domaines.Compte;
using MeepleZone.Domaines.Compte.Contrats.Enums;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace MeepleZone.Site.Authentification;

/// <summary>
/// Crochet <c>OnCreatingTicket</c> des fournisseurs OAuth : délègue au domaine <c>Compte</c> et remplace
/// l'identité du fournisseur par celle de l'utilisateur de la base avant la création du cookie.
/// </summary>
public static class GestionConnexionCompte
{
    // Clé et valeur du marqueur de création forcée (AuthenticationProperties.Items). Seule cette valeur exacte est acceptée.
    public const string CleForcerCreation = "ForcerCreation";

    public const string ValeurForcerCreation = "true";

    public static async Task Traiter(OAuthCreatingTicketContext context)
    {
        var forcerCreation = context.Properties.Items.TryGetValue(CleForcerCreation, out var valeur) && valeur == ValeurForcerCreation;
        var requete = new ExtracteurRevendications().Extraire(context.Scheme.Name, context.Principal!, forcerCreation);
        var domaine = context.HttpContext.RequestServices.GetRequiredService<IDomaineCompte>();

        var reponse = await domaine.GererConnexionAsync(requete);

        switch (reponse.Statut)
        {
            case StatutConnexion.Connecte when reponse.Utilisateur is not null:
                context.Principal = new ConstructeurIdentite().Construire(reponse.Utilisateur, context.Scheme.Name);
                break;
            case StatutConnexion.CompteProbable:
                throw new CompteProbableException();
            default:
                throw new InvalidOperationException("Réponse de connexion inattendue.");
        }
    }
}

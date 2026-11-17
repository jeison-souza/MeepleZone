using MeepleZone.Domaines.Compte.Contrats.Modeles;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace MeepleZone.Site.Authentification.Liaison;

/// <summary>
/// Liaison d'un fournisseur de connexion à un compte déjà connecté. Le défi OAuth part de <c>/liaison/{fournisseur}</c>
/// avec un marqueur dans les propriétés (protégées par le fournisseur d'état, donc non falsifiables); au retour,
/// <see cref="Traiter"/> lie le compte externe à l'utilisateur du cookie en place, sans créer ni remplacer le cookie
/// et sans passer par <c>GererConnexion</c>.
/// </summary>
public static class GestionLiaisonCompte
{
    public const string CleLiaison = "Liaison";

    public const string ValeurLiaison = "true";

    public const string PageRetour = "/profil";

    public static AuthenticationProperties CreerProprietes()
    {
        var proprietes = new AuthenticationProperties { RedirectUri = PageRetour };
        proprietes.Items[CleLiaison] = ValeurLiaison;

        return proprietes;
    }

    /// <summary>Vrai si ces propriétés viennent d'un défi de liaison (seule la valeur exacte « true » compte).</summary>
    public static bool EstLiaison(AuthenticationProperties? proprietes)
    {
        return proprietes is not null
            && proprietes.Items.TryGetValue(CleLiaison, out var valeur)
            && valeur == ValeurLiaison;
    }

    /// <summary>Adresse du profil avec l'issue; le fournisseur vient de la liste fermée, jamais d'un texte libre.</summary>
    public static string UrlRetour(ResultatLiaison resultat, string? fournisseur)
    {
        var url = $"{PageRetour}?{ResultatLiaisonExtensions.Parametre}={resultat.Valeur()}";

        if (FournisseursConnexion.Trouver(fournisseur) is { } connu)
        {
            url += $"&fournisseur={connu.Nom}";
        }

        return url;
    }

    /// <summary>Crochet <c>OnTicketReceived</c> : ne fait rien hors liaison.</summary>
    public static async Task Traiter(TicketReceivedContext context)
    {
        if (!EstLiaison(context.Properties))
        {
            return;
        }

        var fournisseur = context.Scheme.Name;
        var resultat = ResultatLiaison.Echec;

        try
        {
            // Sur l'adresse de retour du fournisseur, l'utilisateur n'est pas encore chargé : on lit le cookie.
            var cookie = await context.HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!cookie.Succeeded || cookie.Principal is null)
            {
                context.Response.Redirect("/connexion-requise");
                context.HandleResponse();

                return;
            }

            // IUtilisateurCourant lit cet utilisateur pendant l'opération du domaine.
            context.HttpContext.User = cookie.Principal;

            var requete = new ExtracteurRevendications().Extraire(fournisseur, context.Principal!, false);
            var liaison = context.HttpContext.RequestServices.GetRequiredService<ILiaisonFournisseur>();

            var reponse = await liaison.LierAsync(new LierFournisseurRequete(requete.Fournisseur, requete.IdentifiantCompteExterne, requete.Courriel));

            resultat = ResultatLiaisonExtensions.De(reponse.Statut);
        }
        catch (Exception exception)
        {
            context.HttpContext.RequestServices
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger(typeof(GestionLiaisonCompte))
                .LogWarning(exception, "Liaison du fournisseur {Fournisseur} impossible.", fournisseur);
        }

        context.Response.Redirect(UrlRetour(resultat, fournisseur));

        // Évite la création du cookie du fournisseur : l'utilisateur reste connecté avec son compte.
        context.HandleResponse();
    }
}

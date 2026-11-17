using MeepleZone.Domaines.Compte;
using MeepleZone.Domaines.Compte.Contrats.Enums;
using MeepleZone.Domaines.Compte.Contrats.Modeles;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace MeepleZone.Site.Authentification.Suppression;

/// <summary>
/// Suppression du compte depuis le profil. Le formulaire envoie le code d'amitié tapé par la personne; le serveur le
/// compare de nouveau au vrai code avant d'appeler le domaine (la vérification de l'écran n'est qu'un confort).
/// Après une suppression réussie, le cookie est retiré et la personne arrive sur la page d'adieu.
/// </summary>
public static class GestionSuppressionCompte
{
    public const string Parametre = "suppression";

    public const string PageProfil = "/profil";

    public const string PageAdieu = "/compte-supprime";

    public const string ChampConfirmation = "confirmation";

    /// <summary>Vrai si la saisie est le code d'amitié attendu (espaces autour et casse ignorés).</summary>
    public static bool ConfirmationValide(string? saisie, string? codeAttendu)
    {
        return !string.IsNullOrWhiteSpace(saisie)
            && !string.IsNullOrWhiteSpace(codeAttendu)
            && string.Equals(saisie.Trim(), codeAttendu, StringComparison.OrdinalIgnoreCase);
    }

    public static string UrlProfil(string motif)
    {
        return $"{PageProfil}?{Parametre}={motif}";
    }

    public static async Task<IResult> Traiter(string? confirmation, IDomaineCompte domaine, ILogger journal)
    {
        try
        {
            var profil = await domaine.ObtenirProfilAsync(new ObtenirProfilRequete());

            // Le compte n'existe plus : la session n'a plus de sens.
            if (profil is null)
            {
                return SortirVers("/");
            }

            if (!ConfirmationValide(confirmation, profil.CodeAmitie))
            {
                return Results.Redirect(UrlProfil("confirmation"));
            }

            var reponse = await domaine.SupprimerCompteAsync(new SupprimerCompteRequete());

            return reponse.Statut switch
            {
                StatutSuppressionCompte.Supprime => SortirVers(PageAdieu),
                StatutSuppressionCompte.CompteProtege => Results.Redirect(UrlProfil("protege")),
                StatutSuppressionCompte.Conflit => Results.Redirect(UrlProfil("conflit")),
                _ => SortirVers("/"),
            };
        }
        catch (Exception exception)
        {
            journal.LogError(exception, "Suppression du compte impossible.");

            return Results.Redirect(UrlProfil("erreur"));
        }
    }

    private static IResult SortirVers(string adresse)
    {
        return Results.SignOut(new AuthenticationProperties { RedirectUri = adresse }, [CookieAuthenticationDefaults.AuthenticationScheme]);
    }

    /// <summary>Message du profil pour le motif de l'adresse, ou <c>null</c> s'il est absent ou inconnu.</summary>
    public static string? Message(string? motif)
    {
        return motif switch
        {
            "confirmation" => "Le code saisi ne correspond pas à votre code d'amitié. Le compte n'a pas été supprimé.",
            "protege" => "Ce compte est protégé : il ne peut pas être supprimé par ce parcours.",
            "conflit" => "Le compte a été modifié pendant la suppression. Rien n'a été supprimé; réessayez.",
            "erreur" => "La suppression n'a pas pu être faite. Rien n'a été supprimé; réessayez dans un instant.",
            _ => null,
        };
    }
}

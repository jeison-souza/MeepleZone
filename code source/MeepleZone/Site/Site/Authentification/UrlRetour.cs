using System;
using System.Linq;

namespace MeepleZone.Site.Authentification;

/// <summary>
/// Valide l'adresse où revenir après une connexion. Seul un chemin local du site est accepté : toute autre valeur
/// (autre site, schéma, caractères de contrôle, pages de connexion) est remplacée par l'accueil. Sans cette vérification,
/// un lien piégé pourrait envoyer la personne vers un site étranger juste après sa connexion (redirection ouverte).
/// </summary>
public static class UrlRetour
{
    public const string Defaut = "/";

    private const int LongueurMax = 512;

    // Retourner sur une page de connexion formerait une boucle.
    private static readonly string[] PrefixesInterdits = ["/connexion", "/deconnexion"];

    public static string Valider(string? url)
    {
        if (string.IsNullOrEmpty(url) || url.Length > LongueurMax)
        {
            return Defaut;
        }

        // Un chemin local commence par une seule barre oblique : « // » et « /\ » sont lus par les navigateurs
        // comme une adresse vers un autre site.
        if (url[0] != '/' || (url.Length > 1 && (url[1] == '/' || url[1] == '\\')))
        {
            return Defaut;
        }

        if (url.Any(caractere => char.IsControl(caractere) || caractere == '\\'))
        {
            return Defaut;
        }

        if (PrefixesInterdits.Any(prefixe => url.StartsWith(prefixe, StringComparison.OrdinalIgnoreCase)))
        {
            return Defaut;
        }

        return url;
    }
}

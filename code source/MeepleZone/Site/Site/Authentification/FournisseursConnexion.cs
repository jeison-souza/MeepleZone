using System;
using System.Collections.Generic;
using System.Linq;

namespace MeepleZone.Site.Authentification;

/// <summary>
/// Liste fermée des fournisseurs de connexion. Les pages en tirent leurs boutons et reconnaissent ici le fournisseur
/// reçu dans l'adresse : une valeur absente de la liste est ignorée, jamais affichée telle quelle.
/// Ajouter un fournisseur (ex. GitHub) commence par une ligne ici.
/// </summary>
public static class FournisseursConnexion
{
    public static IReadOnlyList<FournisseurConnexion> Tous { get; } =
    [
        new("Google", "Google"),
        new("Microsoft", "Microsoft")
    ];

    /// <summary>Retourne le fournisseur de ce nom (casse ignorée), ou <c>null</c> s'il n'est pas dans la liste.</summary>
    public static FournisseurConnexion? Trouver(string? nom)
    {
        return Tous.FirstOrDefault(fournisseur => string.Equals(fournisseur.Nom, nom, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Les fournisseurs autres que celui-là; si le nom est inconnu, la liste complète.</summary>
    public static IReadOnlyList<FournisseurConnexion> Autres(string? nom)
    {
        FournisseurConnexion? exclu = Trouver(nom);

        return Tous.Where(fournisseur => fournisseur != exclu).ToList();
    }
}

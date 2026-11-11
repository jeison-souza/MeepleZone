using System;
using Microsoft.AspNetCore.Authentication;

namespace MeepleZone.Site.Authentification;

/// <summary>
/// Construit les propriétés du défi de connexion à partir de l'adresse demandée. Le paramètre « creation » n'est pas
/// un contrôle de sécurité (l'authentification reste chez le fournisseur) : seule la valeur exacte « true » est prise
/// en compte, toute autre est ignorée.
/// </summary>
public static class ProprietesConnexion
{
    public static AuthenticationProperties Creer(string? retour, string? creation)
    {
        var proprietes = new AuthenticationProperties { RedirectUri = UrlRetour.Valider(retour) };

        if (string.Equals(creation, GestionConnexionCompte.ValeurForcerCreation, StringComparison.Ordinal))
        {
            proprietes.Items[GestionConnexionCompte.CleForcerCreation] = GestionConnexionCompte.ValeurForcerCreation;
        }

        return proprietes;
    }
}

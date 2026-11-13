using System;
using MeepleZone.Site.Authentification.Extensions;
using MeepleZone.Systeme.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;

namespace MeepleZone.Site.Authentification;

/// <summary>
/// Identifiant de l'utilisateur de la session, lu dans la revendication du cookie.
/// Le fournisseur d'état d'authentification du circuit Blazor Server a déjà l'état en mémoire :
/// la lecture est immédiate, sans accès réseau.
/// </summary>
public sealed class UtilisateurCourant(AuthenticationStateProvider fournisseurEtat) : IUtilisateurCourant
{
    private readonly AuthenticationStateProvider _fournisseurEtat = fournisseurEtat;

    public Guid Id
    {
        get
        {
            var etat = _fournisseurEtat.GetAuthenticationStateAsync().GetAwaiter().GetResult();
            return etat.User.Id() ?? throw new InvalidOperationException("Aucun utilisateur authentifié.");
        }
    }
}

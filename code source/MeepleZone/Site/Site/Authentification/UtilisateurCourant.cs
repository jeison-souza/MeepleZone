using MeepleZone.Site.Authentification.Extensions;
using MeepleZone.Systeme.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using System;

namespace MeepleZone.Site.Authentification;

/// <summary>
/// Utilisateur de la session. Dans une requête HTTP ordinaire (par exemple le retour du fournisseur OAuth, hors circuit
/// Blazor), il vient de <c>HttpContext.User</c>; sinon, de l'état d'authentification de Blazor.
/// </summary>
public sealed class UtilisateurCourant(AuthenticationStateProvider fournisseurEtat, IHttpContextAccessor accesHttp) : IUtilisateurCourant
{
    private readonly AuthenticationStateProvider _fournisseurEtat = fournisseurEtat;
    private readonly IHttpContextAccessor _accesHttp = accesHttp;

    public Guid Id
    {
        get
        {
            if (_accesHttp.HttpContext?.User is { Identity.IsAuthenticated: true } utilisateurHttp
                && utilisateurHttp.Id() is { } idHttp)
            {
                return idHttp;
            }

            var etat = _fournisseurEtat.GetAuthenticationStateAsync().GetAwaiter().GetResult();

            return etat.User.Id() ?? throw new InvalidOperationException("Aucun utilisateur authentifié.");
        }
    }
}

using System;
using System.Security.Claims;

namespace MeepleZone.Site.Authentification.Extensions;

/// <summary>Lecture des revendications du cookie du compte (posées par <see cref="ConstructeurIdentite"/>).</summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>Identifiant du compte, ou <c>null</c> si la revendication est absente ou n'est pas un GUID.</summary>
    public static Guid? Id(this ClaimsPrincipal utilisateur)
    {
        return Guid.TryParse(utilisateur.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid id) ? id : null;
    }

    public static string? NomAffichage(this ClaimsPrincipal utilisateur)
    {
        return utilisateur.FindFirst(ClaimTypes.Name)?.Value;
    }

    public static string? Courriel(this ClaimsPrincipal utilisateur)
    {
        return utilisateur.FindFirst(ClaimTypes.Email)?.Value;
    }
}

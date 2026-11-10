using MeepleZone.Domaines.Compte.Contrats.Modeles;
using System.Collections.Generic;
using System.Security.Claims;

namespace MeepleZone.Site.Authentification;

public class ConstructeurIdentite
{
    public ClaimsPrincipal Construire(UtilisateurConnecteDto utilisateur, string nomSchema)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, utilisateur.Id.ToString()),
            new(ClaimTypes.Name, utilisateur.NomAffichage),
            new(ClaimTypes.Role, utilisateur.Role)
        };

        if (!string.IsNullOrWhiteSpace(utilisateur.Courriel))
        {
            claims.Add(new(ClaimTypes.Email, utilisateur.Courriel));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, nomSchema));
    }
}

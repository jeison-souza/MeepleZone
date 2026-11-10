using MeepleZone.Domaines.Compte.Contrats.Modeles;
using System;
using System.Linq;
using System.Security.Claims;

namespace MeepleZone.Site.Authentification;

public class ExtracteurRevendications
{
    public GererConnexionRequete Extraire(string fournisseur, ClaimsPrincipal principal, bool forcerCreation)
    {
        var revendications = principal.Claims;
        var courriel = TexteVideANull(revendications.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value);
        var nomAffichage = TexteVideANull(revendications.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value);
        var identifiantCompteExterne = TexteVideANull(revendications.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);

        if (string.IsNullOrWhiteSpace(identifiantCompteExterne))
        {
            throw new InvalidOperationException("L'identifiant du compte externe est requis.");
        }

        return new GererConnexionRequete(fournisseur, identifiantCompteExterne, courriel, nomAffichage, forcerCreation);
    }

    private static string? TexteVideANull(string? texte)
    {
        if (string.IsNullOrWhiteSpace(texte))
        {
            return null;
        }

        return texte;
    }

}

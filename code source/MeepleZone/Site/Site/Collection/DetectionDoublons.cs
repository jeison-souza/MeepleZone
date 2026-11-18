using MeepleZone.Domaines.Collection;
using MeepleZone.Domaines.Collection.Contrats.Modeles;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MeepleZone.Site.Collection;

/// <summary>Jeux du catalogue dont le nom ressemble à celui qu'une personne s'apprête à créer (ADR 0015).</summary>
public interface IDetectionDoublons
{
    Task<IReadOnlyList<JeuResumeDto>> TrouverAsync(string nom);
}

/// <summary>
/// Provisoire : s'appuie sur la recherche du catalogue (nom qui contient le texte, sans accents ni casse).
/// Ne rattrape pas les fautes de frappe : à remplacer par la détection du domaine (pg_trgm, issue #134).
/// </summary>
public sealed class DetectionDoublonsParRecherche(IDomaineCollection domaine) : IDetectionDoublons
{
    public const int LongueurMinimale = 3;
    public const int NombreMaximal = 5;

    public async Task<IReadOnlyList<JeuResumeDto>> TrouverAsync(string nom)
    {
        var texte = nom?.Trim();

        if (string.IsNullOrEmpty(texte) || texte.Length < LongueurMinimale)
        {
            return [];
        }

        var reponse = await domaine.RechercherJeuxAsync(new RechercherJeuxRequete(texte, 1, NombreMaximal));

        return reponse.Jeux;
    }
}

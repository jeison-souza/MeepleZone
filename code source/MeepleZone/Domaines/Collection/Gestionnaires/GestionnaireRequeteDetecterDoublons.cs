using MeepleZone.Domaines.Collection.Contrats.Modeles;
using MeepleZone.Domaines.Collection.Extensions;
using MeepleZone.Systeme.Depots;
using MeepleZone.Systeme.Interfaces;
using MeepleZone.Systeme.Interfaces.Gestionnaires;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeepleZone.Domaines.Collection.Gestionnaires;

public class GestionnaireRequeteDetecterDoublons(IDepotCollection depotCollection, IUtilisateurCourant utilisateurCourant)
    : IGestionnaireRequete<IReadOnlyList<JeuResumeDto>, DetecterDoublonsRequete>
{
    /// <summary>
    /// Nombre maximal de jeux semblables montrés à la personne qui crée un jeu.
    /// </summary>
    public const int LimiteResultats = 10;

    private readonly IDepotCollection _depotCollection = depotCollection;

    private readonly IUtilisateurCourant _utilisateurCourant = utilisateurCourant;

    public async Task<IReadOnlyList<JeuResumeDto>> TraiterAsync(DetecterDoublonsRequete requete)
    {
        var nom = requete.Nom?.Trim();
        if (string.IsNullOrEmpty(nom))
        {
            return [];
        }

        var resultats = await _depotCollection.DetecterDoublonsAsync(_utilisateurCourant.Id, nom, LimiteResultats);

        return [.. resultats.Select(r => r.VersDto())];
    }
}

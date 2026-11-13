using MeepleZone.Domaines.Collection.Contrats.Modeles;
using MeepleZone.Domaines.Collection.Extensions;
using MeepleZone.Systeme.Depots;
using MeepleZone.Systeme.Interfaces.Gestionnaires;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeepleZone.Domaines.Collection.Gestionnaires;

public class GestionnaireRequeteListerMecaniques(IDepotCatalogue depotCatalogue)
    : IGestionnaireRequete<IReadOnlyList<MecaniqueDto>, ListerMecaniquesRequete>
{
    private readonly IDepotCatalogue _depotCatalogue = depotCatalogue;

    public async Task<IReadOnlyList<MecaniqueDto>> TraiterAsync(ListerMecaniquesRequete requete)
    {
        var mecaniques = await _depotCatalogue.ListerMecaniquesAsync();
        return [.. mecaniques.Select(m => m.VersDto())];
    }
}

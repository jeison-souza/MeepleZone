using MeepleZone.Domaines.Collection.Contrats.Modeles;
using MeepleZone.Domaines.Collection.Extensions;
using MeepleZone.Systeme.Depots;
using MeepleZone.Systeme.Interfaces.Gestionnaires;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeepleZone.Domaines.Collection.Gestionnaires;

public class GestionnaireRequeteListerModesJeu(IDepotCatalogue depotCatalogue)
    : IGestionnaireRequete<IReadOnlyList<ModeJeuDto>, ListerModesJeuRequete>
{
    private readonly IDepotCatalogue _depotCatalogue = depotCatalogue;

    public async Task<IReadOnlyList<ModeJeuDto>> TraiterAsync(ListerModesJeuRequete requete)
    {
        var modesJeu = await _depotCatalogue.ListerModesJeuAsync();
        return [.. modesJeu.Select(m => m.VersDto())];
    }
}

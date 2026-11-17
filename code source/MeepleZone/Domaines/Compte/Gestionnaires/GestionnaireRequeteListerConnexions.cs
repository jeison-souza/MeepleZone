using MeepleZone.Domaines.Compte.Contrats.Modeles;
using MeepleZone.Domaines.Compte.Extensions;
using MeepleZone.Systeme.Depots;
using MeepleZone.Systeme.Interfaces;
using MeepleZone.Systeme.Interfaces.Gestionnaires;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeepleZone.Domaines.Compte.Gestionnaires;

public class GestionnaireRequeteListerConnexions(IDepotCompte depotCompte, IUtilisateurCourant utilisateurCourant)
    : IGestionnaireRequete<IReadOnlyList<ConnexionDto>, ListerConnexionsRequete>
{
    private readonly IDepotCompte _depotCompte = depotCompte;

    private readonly IUtilisateurCourant _utilisateurCourant = utilisateurCourant;

    public async Task<IReadOnlyList<ConnexionDto>> TraiterAsync(ListerConnexionsRequete requete)
    {
        var identites = await _depotCompte.ListerIdentitesAsync(_utilisateurCourant.Id);
        return [.. identites.Select(i => i.VersDto())];
    }
}

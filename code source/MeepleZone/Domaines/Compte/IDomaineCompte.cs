using MeepleZone.Domaines.Compte.Contrats.Modeles;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MeepleZone.Domaines.Compte;

public interface IDomaineCompte
{
    Task<GererConnexionReponse> GererConnexionAsync(GererConnexionRequete requete);

    Task<IReadOnlyList<ConnexionDto>> ListerConnexionsAsync(ListerConnexionsRequete requete);
}

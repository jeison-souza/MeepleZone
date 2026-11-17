using MeepleZone.Domaines.Compte.Contrats.Modeles;
using MeepleZone.Systeme.Interfaces.Gestionnaires;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MeepleZone.Domaines.Compte;

public class DomaineCompte(
    IGestionnaireRequete<GererConnexionReponse, GererConnexionRequete> gestionnaireGererConnexion,
    IGestionnaireRequete<IReadOnlyList<ConnexionDto>, ListerConnexionsRequete> gestionnaireListerConnexions)
    : IDomaineCompte
{
    private readonly IGestionnaireRequete<GererConnexionReponse, GererConnexionRequete> _gestionnaireGererConnexion = gestionnaireGererConnexion;

    private readonly IGestionnaireRequete<IReadOnlyList<ConnexionDto>, ListerConnexionsRequete> _gestionnaireListerConnexions = gestionnaireListerConnexions;

    public Task<GererConnexionReponse> GererConnexionAsync(GererConnexionRequete requete)
    {
        return _gestionnaireGererConnexion.TraiterAsync(requete);
    }

    public Task<IReadOnlyList<ConnexionDto>> ListerConnexionsAsync(ListerConnexionsRequete requete)
    {
        return _gestionnaireListerConnexions.TraiterAsync(requete);
    }
}

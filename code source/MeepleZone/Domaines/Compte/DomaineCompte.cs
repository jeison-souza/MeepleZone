using MeepleZone.Domaines.Compte.Contrats.Modeles;
using MeepleZone.Systeme.Interfaces.Gestionnaires;
using System.Threading.Tasks;

namespace MeepleZone.Domaines.Compte;

public class DomaineCompte(IGestionnaireRequete<GererConnexionReponse, GererConnexionRequete> gestionnaireGererConnexion) : IDomaineCompte
{
    private readonly IGestionnaireRequete<GererConnexionReponse, GererConnexionRequete> _gestionnaireGererConnexion = gestionnaireGererConnexion;

    public Task<GererConnexionReponse> GererConnexionAsync(GererConnexionRequete requete)
    {
        return _gestionnaireGererConnexion.TraiterAsync(requete);
    }
}

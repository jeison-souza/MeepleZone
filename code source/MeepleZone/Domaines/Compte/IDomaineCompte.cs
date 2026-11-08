using MeepleZone.Domaines.Compte.Contrats.Modeles;
using System.Threading.Tasks;

namespace MeepleZone.Domaines.Compte;

public interface IDomaineCompte
{
    Task<GererConnexionReponse> GererConnexionAsync(GererConnexionRequete requete);
}

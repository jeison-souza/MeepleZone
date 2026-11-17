using MeepleZone.Domaines.Compte.Contrats.Modeles;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MeepleZone.Domaines.Compte;

public interface IDomaineCompte
{
    Task<GererConnexionReponse> GererConnexionAsync(GererConnexionRequete requete);

    Task<LierFournisseurReponse> LierFournisseurAsync(LierFournisseurRequete requete);

    Task<IReadOnlyList<ConnexionDto>> ListerConnexionsAsync(ListerConnexionsRequete requete);

    Task<ProfilDto?> ObtenirProfilAsync(ObtenirProfilRequete requete);

    Task<SupprimerCompteReponse> SupprimerCompteAsync(SupprimerCompteRequete requete);
}

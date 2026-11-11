using MeepleZone.Domaines.Collection.Contrats.Modeles;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MeepleZone.Domaines.Collection;

public interface IDomaineCollection
{
    Task<ListerCollectionReponse> ListerCollectionAsync(ListerCollectionRequete requete);

    Task<RechercherJeuxReponse> RechercherJeuxAsync(RechercherJeuxRequete requete);

    Task<AjouterJeuReponse> AjouterJeuAsync(AjouterJeuRequete requete);

    Task<CreerJeuReponse> CreerJeuAsync(CreerJeuRequete requete);

    Task<IReadOnlyList<ModeJeuDto>> ListerModesJeuAsync();

    Task<RetirerJeuReponse> RetirerJeuAsync(RetirerJeuRequete requete);
}

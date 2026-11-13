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

    Task<IReadOnlyList<ModeJeuDto>> ListerModesJeuAsync(ListerModesJeuRequete requete);

    Task<IReadOnlyList<MecaniqueDto>> ListerMecaniquesAsync(ListerMecaniquesRequete requete);

    Task<ModifierPossessionReponse> ModifierPossessionAsync(ModifierPossessionRequete requete);

    Task<RetirerJeuReponse> RetirerJeuAsync(RetirerJeuRequete requete);
}

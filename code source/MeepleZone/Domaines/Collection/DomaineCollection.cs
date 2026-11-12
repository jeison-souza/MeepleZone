using MeepleZone.Domaines.Collection.Contrats.Modeles;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MeepleZone.Domaines.Collection;

public class DomaineCollection : IDomaineCollection
{
    public Task<AjouterJeuReponse> AjouterJeuAsync(AjouterJeuRequete requete)
    {
        throw new System.NotImplementedException();
    }

    public Task<CreerJeuReponse> CreerJeuAsync(CreerJeuRequete requete)
    {
        throw new System.NotImplementedException();
    }

    public Task<ListerCollectionReponse> ListerCollectionAsync(ListerCollectionRequete requete)
    {
        throw new System.NotImplementedException();
    }

    public Task<IReadOnlyList<MecaniqueDto>> ListerMecaniquesAsync(ListerMecaniquesRequete requete)
    {
        throw new System.NotImplementedException();
    }

    public Task<IReadOnlyList<ModeJeuDto>> ListerModesJeuAsync(ListerModesJeuRequete requete)
    {
        throw new System.NotImplementedException();
    }

    public Task<ModifierPossessionReponse> ModifierPossessionAsync(ModifierPossessionRequete requete)
    {
        throw new System.NotImplementedException();
    }

    public Task<RechercherJeuxReponse> RechercherJeuxAsync(RechercherJeuxRequete requete)
    {
        throw new System.NotImplementedException();
    }

    public Task<RetirerJeuReponse> RetirerJeuAsync(RetirerJeuRequete requete)
    {
        throw new System.NotImplementedException();
    }
}

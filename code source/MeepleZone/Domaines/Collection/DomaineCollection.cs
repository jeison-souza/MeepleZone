using MeepleZone.Domaines.Collection.Contrats.Modeles;
using MeepleZone.Systeme.Interfaces.Gestionnaires;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MeepleZone.Domaines.Collection;

public class DomaineCollection(
    IGestionnaireRequete<ListerCollectionReponse, ListerCollectionRequete> gestionnaireCollection,
    IGestionnaireRequete<IReadOnlyList<MecaniqueDto>, ListerMecaniquesRequete> gestionnaireMecaniques,
    IGestionnaireRequete<IReadOnlyList<ModeJeuDto>, ListerModesJeuRequete> gestionnaireModesJeu)
    : IDomaineCollection
{
    private readonly IGestionnaireRequete<ListerCollectionReponse, ListerCollectionRequete> _gestionnaireCollection = gestionnaireCollection;

    private readonly IGestionnaireRequete<IReadOnlyList<MecaniqueDto>, ListerMecaniquesRequete> _gestionnaireMecaniques = gestionnaireMecaniques;

    private readonly IGestionnaireRequete<IReadOnlyList<ModeJeuDto>, ListerModesJeuRequete> _gestionnaireModesJeu = gestionnaireModesJeu;

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
        return _gestionnaireCollection.TraiterAsync(requete);
    }

    public Task<IReadOnlyList<MecaniqueDto>> ListerMecaniquesAsync(ListerMecaniquesRequete requete)
    {
        return _gestionnaireMecaniques.TraiterAsync(requete);
    }

    public Task<IReadOnlyList<ModeJeuDto>> ListerModesJeuAsync(ListerModesJeuRequete requete)
    {
        return _gestionnaireModesJeu.TraiterAsync(requete);
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

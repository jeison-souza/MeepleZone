using MeepleZone.Domaines.Collection.Contrats.Modeles;
using MeepleZone.Systeme.Interfaces.Gestionnaires;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MeepleZone.Domaines.Collection;

public class DomaineCollection(
    IGestionnaireRequete<AjouterJeuReponse, AjouterJeuRequete> gestionnaireAjouterJeu,
    IGestionnaireRequete<CreerJeuReponse, CreerJeuRequete> gestionnaireCreerJeu,
    IGestionnaireRequete<ListerCollectionReponse, ListerCollectionRequete> gestionnaireCollection,
    IGestionnaireRequete<IReadOnlyList<MecaniqueDto>, ListerMecaniquesRequete> gestionnaireMecaniques,
    IGestionnaireRequete<IReadOnlyList<ModeJeuDto>, ListerModesJeuRequete> gestionnaireModesJeu,
    IGestionnaireRequete<RechercherJeuxReponse, RechercherJeuxRequete> gestionnaireRechercherJeux,
    IGestionnaireRequete<ModifierPossessionReponse, ModifierPossessionRequete> gestionnaireModifierPossession,
    IGestionnaireRequete<RetirerJeuReponse, RetirerJeuRequete> gestionnaireRetirerJeu)
    : IDomaineCollection
{
    private readonly IGestionnaireRequete<AjouterJeuReponse, AjouterJeuRequete> _gestionnaireAjouterJeu = gestionnaireAjouterJeu;

    private readonly IGestionnaireRequete<CreerJeuReponse, CreerJeuRequete> _gestionnaireCreerJeu = gestionnaireCreerJeu;

    private readonly IGestionnaireRequete<RechercherJeuxReponse, RechercherJeuxRequete> _gestionnaireRechercherJeux = gestionnaireRechercherJeux;

    private readonly IGestionnaireRequete<ListerCollectionReponse, ListerCollectionRequete> _gestionnaireCollection = gestionnaireCollection;

    private readonly IGestionnaireRequete<IReadOnlyList<MecaniqueDto>, ListerMecaniquesRequete> _gestionnaireMecaniques = gestionnaireMecaniques;

    private readonly IGestionnaireRequete<IReadOnlyList<ModeJeuDto>, ListerModesJeuRequete> _gestionnaireModesJeu = gestionnaireModesJeu;

    private readonly IGestionnaireRequete<ModifierPossessionReponse, ModifierPossessionRequete> _gestionnaireModifierPossession = gestionnaireModifierPossession;

    private readonly IGestionnaireRequete<RetirerJeuReponse, RetirerJeuRequete> _gestionnaireRetirerJeu = gestionnaireRetirerJeu;

    public Task<AjouterJeuReponse> AjouterJeuAsync(AjouterJeuRequete requete)
    {
        return _gestionnaireAjouterJeu.TraiterAsync(requete);
    }

    public Task<CreerJeuReponse> CreerJeuAsync(CreerJeuRequete requete)
    {
        return _gestionnaireCreerJeu.TraiterAsync(requete);
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
        return _gestionnaireModifierPossession.TraiterAsync(requete);
    }

    public Task<RechercherJeuxReponse> RechercherJeuxAsync(RechercherJeuxRequete requete)
    {
        return _gestionnaireRechercherJeux.TraiterAsync(requete);
    }

    public Task<RetirerJeuReponse> RetirerJeuAsync(RetirerJeuRequete requete)
    {
        return _gestionnaireRetirerJeu.TraiterAsync(requete);
    }
}

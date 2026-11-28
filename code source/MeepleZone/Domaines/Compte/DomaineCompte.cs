using MeepleZone.Domaines.Compte.Contrats.Modeles;
using MeepleZone.Systeme.Interfaces.Gestionnaires;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MeepleZone.Domaines.Compte;

public class DomaineCompte(
    IGestionnaireRequete<GererConnexionReponse, GererConnexionRequete> gestionnaireGererConnexion,
    IGestionnaireRequete<IReadOnlyList<ConnexionDto>, ListerConnexionsRequete> gestionnaireListerConnexions,
    IGestionnaireRequete<LierFournisseurReponse, LierFournisseurRequete> gestionnaireLierFournisseur,
    IGestionnaireRequete<ModifierVisibiliteReponse, ModifierVisibiliteRequete> gestionnaireModifierVisibilite,
    IGestionnaireRequete<ProfilDto?, ObtenirProfilRequete> gestionnaireObtenirProfil,
    IGestionnaireRequete<RegenererCodeAmitieReponse, RegenererCodeAmitieRequete> gestionnaireRegenererCodeAmitie,
    IGestionnaireRequete<SupprimerCompteReponse, SupprimerCompteRequete> gestionnaireSupprimerCompte)
    : IDomaineCompte
{
    private readonly IGestionnaireRequete<GererConnexionReponse, GererConnexionRequete> _gestionnaireGererConnexion = gestionnaireGererConnexion;

    private readonly IGestionnaireRequete<IReadOnlyList<ConnexionDto>, ListerConnexionsRequete> _gestionnaireListerConnexions = gestionnaireListerConnexions;

    private readonly IGestionnaireRequete<LierFournisseurReponse, LierFournisseurRequete> _gestionnaireLierFournisseur = gestionnaireLierFournisseur;

    private readonly IGestionnaireRequete<ModifierVisibiliteReponse, ModifierVisibiliteRequete> _gestionnaireModifierVisibilite = gestionnaireModifierVisibilite;

    private readonly IGestionnaireRequete<ProfilDto?, ObtenirProfilRequete> _gestionnaireObtenirProfil = gestionnaireObtenirProfil;

    private readonly IGestionnaireRequete<RegenererCodeAmitieReponse, RegenererCodeAmitieRequete> _gestionnaireRegenererCodeAmitie = gestionnaireRegenererCodeAmitie;

    private readonly IGestionnaireRequete<SupprimerCompteReponse, SupprimerCompteRequete> _gestionnaireSupprimerCompte = gestionnaireSupprimerCompte;

    public Task<GererConnexionReponse> GererConnexionAsync(GererConnexionRequete requete)
    {
        return _gestionnaireGererConnexion.TraiterAsync(requete);
    }

    public Task<LierFournisseurReponse> LierFournisseurAsync(LierFournisseurRequete requete)
    {
        return _gestionnaireLierFournisseur.TraiterAsync(requete);
    }

    public Task<IReadOnlyList<ConnexionDto>> ListerConnexionsAsync(ListerConnexionsRequete requete)
    {
        return _gestionnaireListerConnexions.TraiterAsync(requete);
    }

    public Task<ModifierVisibiliteReponse> ModifierVisibiliteAsync(ModifierVisibiliteRequete requete)
    {
        return _gestionnaireModifierVisibilite.TraiterAsync(requete);
    }

    public Task<ProfilDto?> ObtenirProfilAsync(ObtenirProfilRequete requete)
    {
        return _gestionnaireObtenirProfil.TraiterAsync(requete);
    }

    public Task<RegenererCodeAmitieReponse> RegenererCodeAmitieAsync(RegenererCodeAmitieRequete requete)
    {
        return _gestionnaireRegenererCodeAmitie.TraiterAsync(requete);
    }

    public Task<SupprimerCompteReponse> SupprimerCompteAsync(SupprimerCompteRequete requete)
    {
        return _gestionnaireSupprimerCompte.TraiterAsync(requete);
    }
}

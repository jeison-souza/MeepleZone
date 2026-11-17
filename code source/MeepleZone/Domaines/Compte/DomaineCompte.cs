using MeepleZone.Domaines.Compte.Contrats.Modeles;
using MeepleZone.Systeme.Interfaces.Gestionnaires;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MeepleZone.Domaines.Compte;

public class DomaineCompte(
    IGestionnaireRequete<GererConnexionReponse, GererConnexionRequete> gestionnaireGererConnexion,
    IGestionnaireRequete<IReadOnlyList<ConnexionDto>, ListerConnexionsRequete> gestionnaireListerConnexions,
    IGestionnaireRequete<LierFournisseurReponse, LierFournisseurRequete> gestionnaireLierFournisseur,
    IGestionnaireRequete<ProfilDto?, ObtenirProfilRequete> gestionnaireObtenirProfil,
    IGestionnaireRequete<SupprimerCompteReponse, SupprimerCompteRequete> gestionnaireSupprimerCompte)
    : IDomaineCompte
{
    private readonly IGestionnaireRequete<GererConnexionReponse, GererConnexionRequete> _gestionnaireGererConnexion = gestionnaireGererConnexion;

    private readonly IGestionnaireRequete<IReadOnlyList<ConnexionDto>, ListerConnexionsRequete> _gestionnaireListerConnexions = gestionnaireListerConnexions;

    private readonly IGestionnaireRequete<LierFournisseurReponse, LierFournisseurRequete> _gestionnaireLierFournisseur = gestionnaireLierFournisseur;

    private readonly IGestionnaireRequete<ProfilDto?, ObtenirProfilRequete> _gestionnaireObtenirProfil = gestionnaireObtenirProfil;

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

    public Task<ProfilDto?> ObtenirProfilAsync(ObtenirProfilRequete requete)
    {
        return _gestionnaireObtenirProfil.TraiterAsync(requete);
    }

    public Task<SupprimerCompteReponse> SupprimerCompteAsync(SupprimerCompteRequete requete)
    {
        return _gestionnaireSupprimerCompte.TraiterAsync(requete);
    }
}

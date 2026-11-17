using MeepleZone.Domaines.Compte.Contrats.Modeles;
using MeepleZone.Systeme.Depots;
using MeepleZone.Systeme.Interfaces;
using MeepleZone.Systeme.Interfaces.Gestionnaires;
using System.Threading.Tasks;

namespace MeepleZone.Domaines.Compte.Gestionnaires;

public class GestionnaireRequeteObtenirProfil(IDepotCompte depotCompte, IUtilisateurCourant utilisateurCourant)
    : IGestionnaireRequete<ProfilDto?, ObtenirProfilRequete>
{
    private readonly IDepotCompte _depotCompte = depotCompte;

    private readonly IUtilisateurCourant _utilisateurCourant = utilisateurCourant;

    public async Task<ProfilDto?> TraiterAsync(ObtenirProfilRequete requete)
    {
        var utilisateur = await _depotCompte.TrouverUtilisateurAsync(_utilisateurCourant.Id);

        if (utilisateur == null)
        {
            return null;
        }

        return new ProfilDto(utilisateur.NomAffichage, utilisateur.Courriel, utilisateur.CodeAmitie);
    }
}

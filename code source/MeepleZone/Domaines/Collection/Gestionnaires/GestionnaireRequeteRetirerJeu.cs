using MeepleZone.Domaines.Collection.Contrats.Enums;
using MeepleZone.Domaines.Collection.Contrats.Modeles;
using MeepleZone.Systeme.Depots;
using MeepleZone.Systeme.Interfaces;
using MeepleZone.Systeme.Interfaces.Gestionnaires;
using System.Threading.Tasks;

namespace MeepleZone.Domaines.Collection.Gestionnaires;

public class GestionnaireRequeteRetirerJeu(IDepotCollection depotCollection, IUtilisateurCourant utilisateurCourant)
    : IGestionnaireRequete<RetirerJeuReponse, RetirerJeuRequete>
{
    private readonly IDepotCollection _depotCollection = depotCollection;

    private readonly IUtilisateurCourant _utilisateurCourant = utilisateurCourant;

    public async Task<RetirerJeuReponse> TraiterAsync(RetirerJeuRequete requete)
    {
        var retour = await _depotCollection.RetirerPossessionAsync(_utilisateurCourant.Id, requete.PossessionId) ?
            StatutRetrait.Retire :
            StatutRetrait.Introuvable;
        return new RetirerJeuReponse(retour);
    }
}

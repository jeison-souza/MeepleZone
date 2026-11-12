using MeepleZone.Domaines.Collection.Contrats.Modeles;
using MeepleZone.Domaines.Collection.Extensions;
using MeepleZone.Systeme.Depots;
using MeepleZone.Systeme.Interfaces;
using MeepleZone.Systeme.Interfaces.Gestionnaires;
using System.Linq;
using System.Threading.Tasks;

namespace MeepleZone.Domaines.Collection.Gestionnaires;

public class GestionnaireRequeteListerCollection(IDepotCollection depotCollection, IUtilisateurCourant utilisateurCourant)
    : IGestionnaireRequete<ListerCollectionReponse, ListerCollectionRequete>
{
    private readonly IDepotCollection _depotCollection = depotCollection;

    private readonly IUtilisateurCourant _utilisateurCourant = utilisateurCourant;

    public async Task<ListerCollectionReponse> TraiterAsync(ListerCollectionRequete requete)
    {
        var saut = Pagination.CalculerSaut(requete.Page, requete.TaillePage);

        var recherche = requete.Recherche?.Trim();
        if (string.IsNullOrEmpty(recherche))
        {
            recherche = null;
        }

        var page = await _depotCollection.ListerPossessionsAsync(_utilisateurCourant.Id, recherche, saut, requete.TaillePage);

        return new ListerCollectionReponse([.. page.Items.Select(i => i.VersDto())], page.Total);
    }
}

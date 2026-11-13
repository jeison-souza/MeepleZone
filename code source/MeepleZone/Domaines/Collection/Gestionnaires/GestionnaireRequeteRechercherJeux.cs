using MeepleZone.Domaines.Collection.Contrats.Modeles;
using MeepleZone.Domaines.Collection.Extensions;
using MeepleZone.Systeme.Depots;
using MeepleZone.Systeme.Interfaces;
using MeepleZone.Systeme.Interfaces.Gestionnaires;
using System.Linq;
using System.Threading.Tasks;

namespace MeepleZone.Domaines.Collection.Gestionnaires;

public class GestionnaireRequeteRechercherJeux(IDepotCollection depotCollection, IUtilisateurCourant utilisateurCourant)
    : IGestionnaireRequete<RechercherJeuxReponse, RechercherJeuxRequete>
{
    private readonly IDepotCollection _depotCollection = depotCollection;

    private readonly IUtilisateurCourant _utilisateurCourant = utilisateurCourant;

    public async Task<RechercherJeuxReponse> TraiterAsync(RechercherJeuxRequete requete)
    {
        var saut = Pagination.CalculerSaut(requete.Page, requete.TaillePage);

        var texte = requete.Texte?.Trim();
        if (string.IsNullOrEmpty(texte))
        {
            return new RechercherJeuxReponse([], 0);
        }

        var page = await _depotCollection.RechercherJeuxAsync(_utilisateurCourant.Id, texte, saut, requete.TaillePage);

        return new RechercherJeuxReponse([.. page.Items.Select(i => i.VersDto())], page.Total);
    }
}

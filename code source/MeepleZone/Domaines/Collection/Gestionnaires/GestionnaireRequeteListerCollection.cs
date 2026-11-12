using MeepleZone.Domaines.Collection.Contrats.Modeles;
using MeepleZone.Domaines.Collection.Extensions;
using MeepleZone.Systeme.Depots;
using MeepleZone.Systeme.Interfaces;
using MeepleZone.Systeme.Interfaces.Gestionnaires;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MeepleZone.Domaines.Collection.Gestionnaires;

public class GestionnaireRequeteListerCollection(IDepotCollection depotCollection, IUtilisateurCourant utilisateurCourant)
    : IGestionnaireRequete<ListerCollectionReponse, ListerCollectionRequete>
{
    public const int TaillePageMaximale = 50;

    public const int PageMaximale = int.MaxValue / TaillePageMaximale;

    private readonly IDepotCollection _depotCollection = depotCollection;

    private readonly IUtilisateurCourant _utilisateurCourant = utilisateurCourant;

    public async Task<ListerCollectionReponse> TraiterAsync(ListerCollectionRequete requete)
    {
        if (requete.Page < 1 || requete.Page > PageMaximale)
        {
            throw new ArgumentOutOfRangeException(nameof(requete), requete.Page, $"La page doit être comprise entre 1 et {PageMaximale}.");
        }

        if (requete.TaillePage < 1 || requete.TaillePage > TaillePageMaximale)
        {
            throw new ArgumentOutOfRangeException(nameof(requete), requete.TaillePage, $"La taille de page doit être comprise entre 1 et {TaillePageMaximale}.");
        }

        var saut = (requete.Page - 1) * requete.TaillePage;
        var page = await _depotCollection.ListerPossessionsAsync(_utilisateurCourant.Id, requete.Recherche, saut, requete.TaillePage);

        return new ListerCollectionReponse([.. page.Items.Select(i => i.VersDto())], page.Total);
    }
}

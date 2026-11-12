using MeepleZone.Systeme.Depots.Modeles;
using MeepleZone.Systeme.Entites;
using System;
using System.Threading.Tasks;

namespace MeepleZone.Systeme.Depots;

public interface IDepotCollection
{
    /// <summary>
    /// Liste les possessions de l'utilisateur, avec leur jeu chargé (<see cref="Possession.Jeu"/>).
    /// </summary>
    Task<PageDeResultats<Possession>> ListerPossessionsAsync(Guid utilisateurId, string? recherche, int saut, int prendre);

    /// <summary>
    /// Cherche parmi les jeux visibles de l'utilisateur : les jeux validés et ses propres jeux en attente.
    /// </summary>
    Task<PageDeResultats<ResultatRechercheJeu>> RechercherJeuxAsync(Guid utilisateurId, string texte, int saut, int prendre);
}

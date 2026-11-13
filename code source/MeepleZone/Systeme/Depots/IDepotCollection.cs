using MeepleZone.Systeme.Depots.Enums;
using MeepleZone.Systeme.Depots.Modeles;
using MeepleZone.Systeme.Entites;
using System;
using System.Threading.Tasks;

namespace MeepleZone.Systeme.Depots;

public interface IDepotCollection
{
    Task<PageDeResultats<Possession>> ListerPossessionsAsync(Guid utilisateurId, string? recherche, int saut, int prendre);

    Task<PageDeResultats<ResultatRechercheJeu>> RechercherJeuxAsync(Guid utilisateurId, string texte, int saut, int prendre);

    Task<Possession?> ModifierPossessionAsync(Guid utilisateurId, Guid possessionId, short? note, string? commentaire, int partiesJouees);

    Task<bool> RetirerPossessionAsync(Guid utilisateurId, Guid possessionId);

    Task<Jeu?> TrouverJeuVisibleAsync(Guid utilisateurId, Guid jeuId);

    Task<ResultatAjoutPossession> AjouterPossessionAsync(Possession possession);
}

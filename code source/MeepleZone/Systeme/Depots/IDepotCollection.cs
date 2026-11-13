using MeepleZone.Systeme.Depots.Enums;
using MeepleZone.Systeme.Depots.Modeles;
using MeepleZone.Systeme.Entites;
using System;
using System.Collections.Generic;
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

    /// <summary>
    /// Enregistre en une seule fois le jeu, ses mécaniques et la possession de son créateur.
    /// Les propriétés <see cref="Possession.Jeu"/> et de navigation doivent rester vides.
    /// </summary>
    Task<ResultatCreationJeu> CreerJeuAsync(Jeu jeu, IReadOnlyList<Guid> mecaniqueIds, Possession possession);
}

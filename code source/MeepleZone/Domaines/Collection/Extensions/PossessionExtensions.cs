using MeepleZone.Domaines.Collection.Contrats.Modeles;
using MeepleZone.Systeme.Entites;
using System;
using System.Collections.Generic;

namespace MeepleZone.Domaines.Collection.Extensions;

public static class PossessionExtensions
{
    extension(Possession possession)
    {
        /// <summary>
        /// Convertit une possession dont le jeu a été chargé par le dépôt.
        /// </summary>
        public ItemCollectionDto VersDto(string? modeJeuNom = null, IReadOnlyList<string>? mecaniqueNoms = null)
        {
            var jeu = possession.Jeu ?? throw new InvalidOperationException("Le jeu de la possession n'a pas été chargé.");

            return new ItemCollectionDto(
                PossessionId: possession.Id,
                JeuId: jeu.Id,
                AnneePublication: jeu.AnneePublication,
                Commentaire: possession.Commentaire,
                CouvertureId: jeu.CouvertureId,
                CouvertureUrl: jeu.CouvertureUrl,
                DureeMinMinutes: jeu.DureeMinMinutes,
                DureeMaxMinutes: jeu.DureeMaxMinutes,
                JoueursMin: jeu.JoueursMin,
                JoueursMax: jeu.JoueursMax,
                JoueursMeilleur: jeu.JoueursMeilleur,
                MecaniqueNoms: mecaniqueNoms ?? [],
                ModeJeuNom: modeJeuNom,
                Nom: jeu.Nom,
                Note: possession.Note,
                PartiesJouees: possession.PartiesJouees,
                StatutJeu: jeu.Statut);
        }
    }
}

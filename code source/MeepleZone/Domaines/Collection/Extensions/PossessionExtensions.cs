using MeepleZone.Domaines.Collection.Contrats.Modeles;
using MeepleZone.Systeme.Entites;
using System;

namespace MeepleZone.Domaines.Collection.Extensions;

public static class PossessionExtensions
{
    extension(Possession possession)
    {
        /// <summary>
        /// Convertit une possession dont le jeu a été chargé par le dépôt.
        /// </summary>
        public ItemCollectionDto VersDto()
        {
            var jeu = possession.Jeu ?? throw new InvalidOperationException("Le jeu de la possession n'a pas été chargé.");

            return new ItemCollectionDto(
                possession.Id,
                jeu.Id,
                jeu.Nom,
                jeu.AnneePublication,
                jeu.JoueursMin,
                jeu.JoueursMax,
                jeu.DureeMinMinutes,
                jeu.DureeMaxMinutes,
                jeu.CouvertureId,
                jeu.CouvertureUrl,
                possession.Note,
                possession.PartiesJouees,
                possession.Commentaire,
                jeu.Statut);
        }
    }
}

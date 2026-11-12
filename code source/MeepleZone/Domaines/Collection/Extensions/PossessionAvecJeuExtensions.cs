using MeepleZone.Domaines.Collection.Contrats.Modeles;
using MeepleZone.Systeme.Depots;

namespace MeepleZone.Domaines.Collection.Extensions;

public static class PossessionAvecJeuExtensions
{
    extension(PossessionAvecJeu possessionAvecJeu)
    {
        public ItemCollectionDto VersDto()
        {
            var possession = possessionAvecJeu.Possession;
            var jeu = possessionAvecJeu.Jeu;

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

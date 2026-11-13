using MeepleZone.Domaines.Collection.Contrats.Modeles;
using MeepleZone.Systeme.Depots.Modeles;

namespace MeepleZone.Domaines.Collection.Extensions;

public static class ResultatRechercheJeuExtensions
{
    extension(ResultatRechercheJeu resultat)
    {
        public JeuResumeDto VersDto()
        {
            var jeu = resultat.Jeu;

            return new JeuResumeDto(jeu.Id, jeu.Nom, jeu.AnneePublication, jeu.Editeur, resultat.DejaPossede);
        }
    }
}

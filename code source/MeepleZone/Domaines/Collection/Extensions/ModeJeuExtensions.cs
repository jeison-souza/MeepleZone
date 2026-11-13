using MeepleZone.Domaines.Collection.Contrats.Modeles;
using MeepleZone.Systeme.Entites;

namespace MeepleZone.Domaines.Collection.Extensions;

public static class ModeJeuExtensions
{
    extension(ModeJeu modeJeu)
    {
        public ModeJeuDto VersDto()
        {
            return new ModeJeuDto(modeJeu.Id, modeJeu.Nom, modeJeu.Description);
        }
    }
}

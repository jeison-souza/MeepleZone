using MeepleZone.Domaines.Compte.Contrats.Modeles;
using MeepleZone.Systeme.Entites;

namespace MeepleZone.Domaines.Compte.Extensions;

public static class IdentiteExterneExtensions
{
    extension(IdentiteExterne identite)
    {
        public ConnexionDto VersDto()
        {
            return new ConnexionDto(identite.Fournisseur, identite.Courriel, identite.CreeLe);
        }
    }
}

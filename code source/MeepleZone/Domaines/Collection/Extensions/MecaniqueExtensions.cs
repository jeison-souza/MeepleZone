using MeepleZone.Domaines.Collection.Contrats.Modeles;
using MeepleZone.Systeme.Entites;

namespace MeepleZone.Domaines.Collection.Extensions;

public static class MecaniqueExtensions
{
    extension(Mecanique mecanique)
    {
        public MecaniqueDto VersDto()
        {
            return new MecaniqueDto(mecanique.Id, mecanique.Nom, mecanique.Description);
        }
    }
}

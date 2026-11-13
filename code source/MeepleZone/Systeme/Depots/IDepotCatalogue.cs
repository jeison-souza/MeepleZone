using MeepleZone.Systeme.Entites;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MeepleZone.Systeme.Depots;

public interface IDepotCatalogue
{
    Task<IReadOnlyList<ModeJeu>> ListerModesJeuAsync();

    Task<IReadOnlyList<Mecanique>> ListerMecaniquesAsync();
}

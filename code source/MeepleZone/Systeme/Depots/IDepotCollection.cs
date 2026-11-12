using System;
using System.Threading.Tasks;

namespace MeepleZone.Systeme.Depots;

public interface IDepotCollection
{
    Task<PageDePossessions> ListerPossessionsAsync(Guid utilisateurId, string? recherche, int saut, int prendre);
}

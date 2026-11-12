using MeepleZone.Systeme.Depots;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MeepleZone.Systeme.Donnees.Depots;

public class DepotCollection(IDbContextFactory<MeepleZoneContexte> fabrique) : IDepotCollection
{
    private readonly IDbContextFactory<MeepleZoneContexte> _fabrique = fabrique;

    public async Task<PageDePossessions> ListerPossessionsAsync(Guid utilisateurId, string? recherche, int saut, int prendre)
    {
        await using var contexte = await _fabrique.CreateDbContextAsync();

        var possessions = contexte.Possessions
            .AsNoTracking()
            .Where(p => p.UtilisateurId == utilisateurId)
            .Join(contexte.Jeux, p => p.JeuId, j => j.Id, (p, j) => new { Possession = p, Jeu = j });

        var total = await possessions.CountAsync();

        var items = await possessions
            .OrderBy(x => x.Jeu.Nom)
            .ThenBy(x => x.Possession.Id)
            .Skip(saut)
            .Take(prendre)
            .ToListAsync();

        return new PageDePossessions([.. items.Select(x => new PossessionAvecJeu(x.Possession, x.Jeu))], total);
    }
}

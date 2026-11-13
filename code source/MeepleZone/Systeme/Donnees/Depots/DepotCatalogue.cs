using MeepleZone.Systeme.Depots;
using MeepleZone.Systeme.Entites;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeepleZone.Systeme.Donnees.Depots;

public class DepotCatalogue(IDbContextFactory<MeepleZoneContexte> fabrique) : IDepotCatalogue
{
    private readonly IDbContextFactory<MeepleZoneContexte> _fabrique = fabrique;

    public async Task<IReadOnlyList<Mecanique>> ListerMecaniquesAsync()
    {
        await using var contexte = await _fabrique.CreateDbContextAsync();

        return await contexte.Mecaniques
            .AsNoTracking()
            .OrderBy(m => m.Nom)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ModeJeu>> ListerModesJeuAsync()
    {
        await using var contexte = await _fabrique.CreateDbContextAsync();

        return await contexte.ModesJeu
            .AsNoTracking()
            .OrderBy(m => m.Nom)
            .ToListAsync();
    }
}

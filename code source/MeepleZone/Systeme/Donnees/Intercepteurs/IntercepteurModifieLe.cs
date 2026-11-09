using MeepleZone.Systeme.Entites.Attributs;
using MeepleZone.Systeme.Entites.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MeepleZone.Systeme.Donnees.Intercepteurs;

public class IntercepteurModifieLe(TimeProvider fournisseurTemps) : SaveChangesInterceptor
{
    private readonly TimeProvider _fournisseurTemps = fournisseurTemps;

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            AppliquerModifierLe(eventData.Context.ChangeTracker);
        }

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            AppliquerModifierLe(eventData.Context.ChangeTracker);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public void AppliquerModifierLe(ChangeTracker suivi)
    {
        var entitesModifiees = suivi.Entries().Where(e => e.State == EntityState.Modified);
        foreach (var e in entitesModifiees)
        {
            // Si possede propriété autre que avec l'attribut [AttributTechnique],
            // on met à jour la propriété ModifieLe avec la date et l'heure actuelle.
            if (e.Entity is IModifieLe &&
                e.Properties.Any(p => p.IsModified &&
                                      p.Metadata
                                       .PropertyInfo?
                                       .IsDefined(typeof(AttributTechnique), false) != true &&
                                      p.Metadata.Name != nameof(IModifieLe.ModifieLe)))
            {
                e.Property(nameof(IModifieLe.ModifieLe)).CurrentValue = _fournisseurTemps.GetUtcNow();
            }
        }
    }
}

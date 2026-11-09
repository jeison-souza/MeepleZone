using MeepleZone.Systeme.Donnees.Depots;
using MeepleZone.Systeme.Donnees.Intercepteurs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace MeepleZone.Systeme.Donnees.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AjouterSystemeDonnees(this IServiceCollection services, string chaineConnexion)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddDbContextFactory<MeepleZoneContexte>((fournisseur, options) => options
                .UtiliserPostgreSql(chaineConnexion)
                .AddInterceptors(new IntercepteurModifieLe(fournisseur.GetRequiredService<TimeProvider>())));
        services.Scan(scan => scan
                .FromAssemblyOf<DepotCompte>()
                .AddClasses(classes => classes.InNamespaceOf<DepotCompte>())
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        return services;
    }
}

using Microsoft.Extensions.DependencyInjection;

namespace MeepleZone.Domaines.Compte.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AjouterDomaineCompte(this IServiceCollection services)
    {
        services.Scan(scan => scan
            .FromAssemblyOf<DomaineCompte>()

            .AddClasses(classes => classes.InNamespaceOf<DomaineCompte>())
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        return services;
    }
}

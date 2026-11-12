using Microsoft.Extensions.DependencyInjection;

namespace MeepleZone.Domaines.Collection.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AjouterDomaineCollection(this IServiceCollection services)
    {
        services.Scan(scan => scan
            .FromAssemblyOf<DomaineCollection>()
            .AddClasses(classes => classes.InNamespaceOf<DomaineCollection>())
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        return services;
    }
}

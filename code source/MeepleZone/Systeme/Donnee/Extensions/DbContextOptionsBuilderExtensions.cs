using Microsoft.EntityFrameworkCore;

namespace MeepleZone.Systeme.Donnee.Extensions;

public static class DbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Configure le contexte pour PostgreSQL avec les noms en snake_case.
    /// Un seul endroit pour ces options : la fabrique de migration, les tests et, plus tard, le site.
    /// </summary>
    public static DbContextOptionsBuilder<MeepleZoneContexte> UtiliserPostgreSql(
        this DbContextOptionsBuilder<MeepleZoneContexte> options,
        string chaineConnexion)
    {
        return options
            .UseNpgsql(chaineConnexion, o => o.MigrationsHistoryTable("historique_migration"))
            .UseSnakeCaseNamingConvention();
    }
}

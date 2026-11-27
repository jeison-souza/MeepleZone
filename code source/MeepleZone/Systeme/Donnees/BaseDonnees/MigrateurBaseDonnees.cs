using MeepleZone.Systeme.Donnees.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MeepleZone.Systeme.Donnees.BaseDonnees;

public static class MigrateurBaseDonnees
{
    // Plus long que le délai par défaut (30 s) : une migration peut attendre un disque lent.
    private const int DelaiCommandeSecondes = 120;

    /// <summary>
    /// Adapte la chaîne du rôle de déploiement : sans bassin de connexions, pour que la connexion
    /// soit réellement fermée après la migration, et avec un délai de commande plus long.
    /// </summary>
    public static string PreparerChaine(string chaineConnexion)
    {
        var constructeur = new NpgsqlConnectionStringBuilder(chaineConnexion)
        {
            Pooling = false,
            CommandTimeout = DelaiCommandeSecondes
        };

        return constructeur.ConnectionString;
    }

    /// <summary>
    /// Applique les migrations en attente avec un contexte propre à cette opération, distinct de
    /// celui de l'application. Un échec est journalisé puis relancé : l'application ne doit pas
    /// démarrer sur une base incomplète.
    /// </summary>
    public static async Task MigrerAsync(string chaineConnexion, ILogger journal)
    {
        try
        {
            var options = new DbContextOptionsBuilder<MeepleZoneContexte>()
                .UtiliserPostgreSql(PreparerChaine(chaineConnexion))
                .Options;

            await using var contexte = new MeepleZoneContexte(options);

            var enAttente = (await contexte.Database.GetPendingMigrationsAsync()).ToList();
            journal.LogInformation(
                "{Nombre} migration(s) en attente : {Migrations}",
                enAttente.Count,
                string.Join(", ", enAttente));

            // EF Core gère lui-même la transaction et le verrou : pas de transaction autour de cet appel.
            await contexte.Database.MigrateAsync();

            journal.LogInformation("La base de données est à jour.");
        }
        catch (Exception exception)
        {
            journal.LogCritical(exception, "La migration de la base de données a échoué.");
            throw;
        }
    }
}

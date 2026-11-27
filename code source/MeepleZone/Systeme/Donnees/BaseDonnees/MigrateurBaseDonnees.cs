using Npgsql;

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
}

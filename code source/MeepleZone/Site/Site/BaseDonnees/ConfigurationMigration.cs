using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;

namespace MeepleZone.Site.BaseDonnees;

public static class ConfigurationMigration
{
    public const string NomChaine = "MeepleZoneMigration";

    public static string? LireChaineConnexion(IConfiguration configuration, IHostEnvironment environnement)
    {
        var chaine = configuration.GetConnectionString(NomChaine);

        if (string.IsNullOrWhiteSpace(chaine))
        {
            if (environnement.IsDevelopment())
            {
                return null;
            }

            throw new InvalidOperationException($"ConnectionStrings:{NomChaine} manquant.");
        }

        return chaine;
    }
}

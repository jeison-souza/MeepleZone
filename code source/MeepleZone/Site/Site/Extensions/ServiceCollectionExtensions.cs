using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Net;

namespace MeepleZone.Site.Extensions;

public static class ServiceCollectionExtensions
{
    public const string SectionEnTetesTransmis = "EnTetesTransmis";
    public const string SectionProtectionDonnees = "ProtectionDonnees";

    /// <summary>
    /// Fait confiance aux en-têtes <c>X-Forwarded-For</c> et <c>X-Forwarded-Proto</c> seulement quand ils viennent
    /// d'un proxy inverse déclaré (<c>EnTetesTransmis:Proxys</c> pour des adresses, <c>EnTetesTransmis:Reseaux</c> pour
    /// des plages CIDR). Sans cela, derrière le proxy, l'application croit être en HTTP et construit des adresses de
    /// rappel de connexion en <c>http://</c>. Sans proxy déclaré, seules les valeurs par défaut du framework
    /// (boucle locale) s'appliquent.
    /// </summary>
    public static IServiceCollection ConfigurerEnTetesTransmis(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(SectionEnTetesTransmis);
        var proxys = section.GetSection("Proxys").Get<string[]>() ?? [];
        var reseaux = section.GetSection("Reseaux").Get<string[]>() ?? [];

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

            if (proxys.Length == 0 && reseaux.Length == 0)
            {
                return;
            }

            // Seuls les proxys déclarés sont crus : on retire les valeurs par défaut.
            options.KnownProxies.Clear();
            options.KnownIPNetworks.Clear();

            foreach (var proxy in proxys)
            {
                options.KnownProxies.Add(IPAddress.Parse(proxy));
            }

            foreach (var reseau in reseaux)
            {
                options.KnownIPNetworks.Add(System.Net.IPNetwork.Parse(reseau));
            }
        });

        return services;
    }

    /// <summary>
    /// Garde les clés de protection des données (cookie de connexion, jeton antiforgery) dans un dossier quand
    /// <c>ProtectionDonnees:Repertoire</c> est renseigné. Sans dossier persistant, chaque redémarrage du conteneur
    /// génère de nouvelles clés : les cookies existants deviennent illisibles et tout le monde est déconnecté.
    /// </summary>
    public static IServiceCollection ConfigurerProtectionDonnees(this IServiceCollection services, IConfiguration configuration)
    {
        var repertoire = configuration[$"{SectionProtectionDonnees}:Repertoire"];

        var constructeur = services.AddDataProtection().SetApplicationName("MeepleZone");

        if (!string.IsNullOrWhiteSpace(repertoire))
        {
            constructeur.PersistKeysToFileSystem(new DirectoryInfo(repertoire));
        }

        return services;
    }
}

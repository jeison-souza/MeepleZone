using Microsoft.Extensions.Configuration;

namespace MeepleZone.Site.Versionnage;

/// <summary>
/// Version de l'application, inscrite dans l'image par la construction (variables <c>Version__Numero</c> et
/// <c>Version__Commit</c>). Hors d'une image construite, les deux sont absentes et la version vaut « dev ».
/// </summary>
public sealed class InformationsVersion
{
    public const string ValeurParDefaut = "dev";

    private const int LongueurCommitAffiche = 7;

    private InformationsVersion(string numero, string commit)
    {
        Numero = numero;
        Commit = commit;
    }

    public string Numero { get; }

    /// <summary>SHA complet de la validation, ou « dev » s'il est inconnu.</summary>
    public string Commit { get; }

    /// <summary>Texte du pied du menu latéral.</summary>
    public string Libelle
    {
        get
        {
            if (Commit == ValeurParDefaut)
            {
                return $"Version {Numero}";
            }

            var commitAffiche = Commit.Length > LongueurCommitAffiche ? Commit[..LongueurCommitAffiche] : Commit;

            return $"Version {Numero} · {commitAffiche}";
        }
    }

    public static InformationsVersion Lire(IConfiguration configuration)
    {
        return new InformationsVersion(LireValeur(configuration, "Version:Numero"), LireValeur(configuration, "Version:Commit"));
    }

    private static string LireValeur(IConfiguration configuration, string cle)
    {
        var valeur = configuration[cle];

        return string.IsNullOrWhiteSpace(valeur) ? ValeurParDefaut : valeur.Trim();
    }
}

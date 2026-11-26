using System.Reflection;

namespace MeepleZone.Site.Versionnage;

/// <summary>
/// Version de l'application, inscrite dans l'assembly à la compilation sous la forme <c>numéro+commit</c>
/// (propriété <c>InformationalVersion</c>), pour qu'un déploiement ne puisse pas la remplacer.
/// Hors du pipeline, la version vaut « dev ».
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

    public static InformationsVersion Lire(Assembly assembly)
    {
        return Lire(assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion);
    }

    /// <summary>Sépare « numéro+commit »; une partie absente ou vide vaut « dev ».</summary>
    public static InformationsVersion Lire(string? versionInformationnelle)
    {
        var parties = (versionInformationnelle ?? string.Empty).Split('+', 2);
        var commit = parties.Length > 1 ? parties[1] : null;

        return new InformationsVersion(Normaliser(parties[0]), Normaliser(commit));
    }

    private static string Normaliser(string? valeur)
    {
        return string.IsNullOrWhiteSpace(valeur) ? ValeurParDefaut : valeur.Trim();
    }
}

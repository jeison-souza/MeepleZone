using System.Text.RegularExpressions;

namespace MeepleZone.Systeme.Donnee.Test;

internal static partial class ConversionsDeNoms
{
    public static string EnSnakeCase(string nomPropriete)
    {
        var mots = MotsCamelCase().Split(nomPropriete);

        return string.Join("_", mots).ToLowerInvariant();
    }

    [GeneratedRegex("(?<!^)(?=[A-Z])")]
    private static partial Regex MotsCamelCase();
}

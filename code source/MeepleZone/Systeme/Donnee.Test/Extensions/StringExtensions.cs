using System.Text.RegularExpressions;

namespace MeepleZone.Systeme.Donnee.Test.Extensions;

internal static partial class StringExtensions
{
    /// <summary>Convertit un nom en PascalCase en snake_case (<c>NomAffichage</c> donne <c>nom_affichage</c>).</summary>
    public static string EnSnakeCase(this string nomPascalCase)
    {
        var mots = MotsPascalCase().Split(nomPascalCase);

        return string.Join("_", mots).ToLowerInvariant();
    }

    [GeneratedRegex("(?<!^)(?=[A-Z])")]
    private static partial Regex MotsPascalCase();
}

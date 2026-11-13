namespace MeepleZone.Site.Affichage;

/// <summary>
/// Met en forme les caractéristiques d'un jeu pour les cartes de la collection (« 2 à 4 joueurs », « 30 à 60 min »).
/// Chaque méthode retourne <c>null</c> quand rien n'est connu : la carte n'affiche alors pas la ligne.
/// </summary>
public static class FormatageJeu
{
    public static string? Joueurs(short? minimum, short? maximum)
    {
        return Plage(minimum, maximum, " joueur", " joueurs");
    }

    public static string? Duree(short? minimum, short? maximum)
    {
        return Plage(minimum, maximum, " min", " min");
    }

    // Une seule valeur connue vaut pour les deux bornes; des bornes égales donnent une valeur unique.
    private static string? Plage(short? minimum, short? maximum, string unite, string unitePluriel)
    {
        var bas = minimum ?? maximum;
        var haut = maximum ?? minimum;

        if (bas is null || haut is null)
        {
            return null;
        }

        if (bas == haut)
        {
            return bas + (bas > 1 ? unitePluriel : unite);
        }

        return $"{bas} à {haut}{unitePluriel}";
    }
}

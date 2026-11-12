using System;

namespace MeepleZone.Domaines.Collection;

public static class Pagination
{
    public const int TaillePageMaximale = 50;

    public const int PageMaximale = int.MaxValue / TaillePageMaximale;

    /// <summary>
    /// Valide la page (base 1) et sa taille, puis retourne le nombre d'éléments à sauter.
    /// </summary>
    public static int CalculerSaut(int page, int taillePage)
    {
        if (page < 1 || page > PageMaximale)
        {
            throw new ArgumentOutOfRangeException(nameof(page), page, $"La page doit être comprise entre 1 et {PageMaximale}.");
        }

        if (taillePage < 1 || taillePage > TaillePageMaximale)
        {
            throw new ArgumentOutOfRangeException(nameof(taillePage), taillePage, $"La taille de page doit être comprise entre 1 et {TaillePageMaximale}.");
        }

        return (page - 1) * taillePage;
    }
}

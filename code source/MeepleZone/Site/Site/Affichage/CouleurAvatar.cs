using System;

namespace MeepleZone.Site.Affichage;

/// <summary>Choisit la couleur de l'avatar d'après l'identifiant du compte : la même personne garde toujours la même couleur.</summary>
public static class CouleurAvatar
{
    /// <summary>
    /// Index (de 0 à <paramref name="nombreCouleurs"/> - 1) dans la palette de l'interface. Il vient du premier octet
    /// de l'identifiant, jamais de <c>GetHashCode</c>, dont la valeur change d'une exécution à l'autre.
    /// </summary>
    public static int Index(Guid id, int nombreCouleurs)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(nombreCouleurs);

        return id.ToByteArray()[0] % nombreCouleurs;
    }
}

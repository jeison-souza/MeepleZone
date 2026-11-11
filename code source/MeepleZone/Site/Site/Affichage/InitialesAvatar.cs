using System;
using System.Linq;
using System.Text;

namespace MeepleZone.Site.Affichage;

/// <summary>
/// Calcule les initiales dessinées dans l'avatar à partir du nom d'affichage. Seules les lettres A à Z comptent
/// (les accents sont retirés : « É » donne « E »); emoji, chiffres et autres écritures sont ignorés.
/// </summary>
public static class InitialesAvatar
{
    /// <summary>
    /// Première lettre du premier et du dernier mot (« Joao vicente de paula » donne « JP »); avec un seul mot,
    /// ses deux premières lettres (« Marcelo » donne « MA »). Retourne <c>null</c> quand rien n'est exploitable :
    /// l'avatar affiche alors l'icône générique.
    /// </summary>
    public static string? Calculer(string? nomAffichage)
    {
        if (string.IsNullOrWhiteSpace(nomAffichage))
        {
            return null;
        }

        var mots = nomAffichage
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
            .Select(LettresMajuscules)
            .Where(mot => mot.Length > 0)
            .ToList();

        if (mots.Count >= 2)
        {
            return new string([mots[0][0], mots[^1][0]]);
        }

        if (mots.Count == 1 && mots[0].Length >= 2)
        {
            return mots[0][..2];
        }

        return null;
    }

    private static string LettresMajuscules(string mot)
    {
        // La forme décomposée sépare la lettre de son accent (« é » devient « e » suivi d'une marque), que le filtre écarte.
        var decompose = mot.Normalize(NormalizationForm.FormD);

        return new string(decompose
            .Where(caractere => caractere is >= 'A' and <= 'Z' or >= 'a' and <= 'z')
            .Select(char.ToUpperInvariant)
            .ToArray());
    }
}

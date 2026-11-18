using MeepleZone.Domaines.Collection.Contrats.Modeles;
using System.Collections.Generic;

namespace MeepleZone.Site.Collection;

public static class PresentationJeu
{
    /// <summary>Ligne de détail d'un jeu de la recherche : année et éditeur, ou une mention quand ils sont inconnus.</summary>
    public static string Detail(JeuResumeDto jeu)
    {
        var elements = new List<string>();

        if (jeu.AnneePublication is { } annee)
        {
            elements.Add(annee.ToString());
        }

        if (!string.IsNullOrWhiteSpace(jeu.Editeur))
        {
            elements.Add(jeu.Editeur);
        }

        return elements.Count > 0 ? string.Join(" · ", elements) : "Année et éditeur inconnus";
    }
}

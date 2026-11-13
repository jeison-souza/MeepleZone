using MeepleZone.Domaines.Collection.Contrats.Modeles;

namespace MeepleZone.Domaines.Collection;

/// <summary>
/// Règles de la création d'un jeu. Elles reprennent les contraintes <c>CHECK</c> de la table <c>jeu</c> (schéma de données),
/// pour refuser une demande invalide avant d'atteindre la base.
/// </summary>
public static class ValidationJeu
{
    public const int NomLongueurMax = 255;

    public const int EditeurLongueurMax = 255;

    public const decimal ComplexiteMin = 1m;

    public const decimal ComplexiteMax = 5m;

    /// <param name="requete">Demande de création.</param>
    /// <param name="nom">Nom déjà normalisé (sans espaces aux extrémités), ou <c>null</c> s'il est vide.</param>
    /// <param name="editeur">Éditeur déjà normalisé, ou <c>null</c>.</param>
    public static bool EstValide(CreerJeuRequete requete, string? nom, string? editeur)
    {
        if (nom == null || nom.Length > NomLongueurMax)
        {
            return false;
        }

        if (editeur != null && editeur.Length > EditeurLongueurMax)
        {
            return false;
        }

        if (requete.JoueursMin < 1 || requete.JoueursMax < 1 || requete.JoueursMeilleur < 1)
        {
            return false;
        }

        if (requete.DureeMinMinutes <= 0 || requete.DureeMaxMinutes <= 0)
        {
            return false;
        }

        if (requete.AgeMin < 0)
        {
            return false;
        }

        if (requete.Complexite < ComplexiteMin || requete.Complexite > ComplexiteMax)
        {
            return false;
        }

        if (requete.JoueursMin > requete.JoueursMax)
        {
            return false;
        }

        if (requete.JoueursMeilleur < requete.JoueursMin || requete.JoueursMeilleur > requete.JoueursMax)
        {
            return false;
        }

        if (requete.DureeMinMinutes > requete.DureeMaxMinutes)
        {
            return false;
        }

        return true;
    }
}

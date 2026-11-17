namespace MeepleZone.Site.Authentification.Liaison;

/// <summary>Issue d'une tentative de liaison, transmise au profil par le paramètre d'adresse « liaison ».</summary>
public enum ResultatLiaison
{
    Reussie,
    DejaLiee,
    DejaUtilisee,
    Echec,
}

public static class ResultatLiaisonExtensions
{
    public const string Parametre = "liaison";

    /// <summary>Valeur du paramètre d'adresse pour cette issue.</summary>
    public static string Valeur(this ResultatLiaison resultat)
    {
        return resultat switch
        {
            ResultatLiaison.Reussie => "reussie",
            ResultatLiaison.DejaLiee => "deja-liee",
            ResultatLiaison.DejaUtilisee => "deja-utilisee",
            _ => "echec",
        };
    }

    /// <summary>Issue correspondant à la valeur du paramètre, ou <c>null</c> si absente ou inconnue.</summary>
    public static ResultatLiaison? Lire(string? valeur)
    {
        return valeur switch
        {
            "reussie" => ResultatLiaison.Reussie,
            "deja-liee" => ResultatLiaison.DejaLiee,
            "deja-utilisee" => ResultatLiaison.DejaUtilisee,
            "echec" => ResultatLiaison.Echec,
            _ => null,
        };
    }

    public static string Message(this ResultatLiaison resultat, string fournisseur)
    {
        return resultat switch
        {
            ResultatLiaison.Reussie => $"Le compte {fournisseur} est maintenant associé à votre compte MeepleZone.",
            ResultatLiaison.DejaLiee => $"Ce compte {fournisseur} est déjà associé à votre compte MeepleZone.",
            ResultatLiaison.DejaUtilisee => $"Ce compte {fournisseur} est déjà utilisé par un autre compte MeepleZone. Pour l'associer à celui-ci, supprimez le compte MeepleZone le plus récent, puis revenez ici.",
            _ => $"L'association du compte {fournisseur} n'a pas pu être faite. Réessayez dans un instant.",
        };
    }
}

using MeepleZone.Domaines.Compte.Contrats.Enums;
using MeepleZone.Systeme.Entites.Constantes;
using System;

namespace MeepleZone.Domaines.Compte.Extensions;

public static class VisibiliteCollectionExtensions
{
    public static string VersValeur(this VisibiliteCollection visibilite)
    {
        return visibilite switch
        {
            VisibiliteCollection.Privee => VisibilitesCollection.Privee,
            VisibiliteCollection.Amis => VisibilitesCollection.Amis,
            _ => throw new ArgumentOutOfRangeException(nameof(visibilite), visibilite, "Visibilité inconnue."),
        };
    }

    public static VisibiliteCollection VersVisibiliteCollection(this string valeur)
    {
        return valeur switch
        {
            VisibilitesCollection.Privee => VisibiliteCollection.Privee,
            VisibilitesCollection.Amis => VisibiliteCollection.Amis,
            _ => throw new ArgumentOutOfRangeException(nameof(valeur), valeur, "Visibilité inconnue."),
        };
    }
}

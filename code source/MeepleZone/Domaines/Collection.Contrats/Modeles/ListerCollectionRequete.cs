namespace MeepleZone.Domaines.Collection.Contrats.Modeles;

public sealed record ListerCollectionRequete(
    string? Recherche,
    int Page,
    int TaillePage
);

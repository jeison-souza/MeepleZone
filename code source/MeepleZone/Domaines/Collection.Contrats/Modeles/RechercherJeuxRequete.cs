namespace MeepleZone.Domaines.Collection.Contrats.Modeles;

public sealed record RechercherJeuxRequete(
    string Texte,
    int Page,
    int TaillePage
);

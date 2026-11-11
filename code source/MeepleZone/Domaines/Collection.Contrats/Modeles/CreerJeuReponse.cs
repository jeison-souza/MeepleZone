using MeepleZone.Domaines.Collection.Contrats.Enums;

namespace MeepleZone.Domaines.Collection.Contrats.Modeles;

public sealed record CreerJeuReponse(
    StatutCreationJeu Statut,
    ItemCollectionDto? Item
);

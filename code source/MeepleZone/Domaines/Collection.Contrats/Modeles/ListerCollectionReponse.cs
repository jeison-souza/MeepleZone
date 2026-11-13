using System.Collections.Generic;

namespace MeepleZone.Domaines.Collection.Contrats.Modeles;

public sealed record ListerCollectionReponse(
    IReadOnlyList<ItemCollectionDto> Items,
    int Total
);

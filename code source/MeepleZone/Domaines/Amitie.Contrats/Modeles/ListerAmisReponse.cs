using System.Collections.Generic;

namespace MeepleZone.Domaines.Amitie.Contrats.Modeles;

public sealed record ListerAmisReponse(
    IReadOnlyList<AmiDto> Items,
    int Total
);

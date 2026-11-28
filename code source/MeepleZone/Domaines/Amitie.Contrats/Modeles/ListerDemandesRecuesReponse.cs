using System.Collections.Generic;

namespace MeepleZone.Domaines.Amitie.Contrats.Modeles;

public sealed record ListerDemandesRecuesReponse(
    IReadOnlyList<DemandeRecueDto> Items,
    int Total
);

using System.Collections.Generic;

namespace MeepleZone.Domaines.Amitie.Contrats.Modeles;

public sealed record ListerDemandesEnvoyeesReponse(
    IReadOnlyList<DemandeEnvoyeeDto> Items,
    int Total
);

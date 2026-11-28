using System;
using MeepleZone.Domaines.Amitie.Contrats.Enums;

namespace MeepleZone.Domaines.Amitie.Contrats.Modeles;

public sealed record DemandeEnvoyeeDto(
    Guid Id,
    string? CodeSaisi,
    string? CourrielSaisi,
    StatutDemandeEnvoyee Statut,
    DateTimeOffset CreeLe
);

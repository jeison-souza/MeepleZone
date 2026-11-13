using System;

namespace MeepleZone.Domaines.Collection.Contrats.Modeles;

public sealed record MecaniqueDto(
    Guid MecaniqueId,
    string Nom,
    string? Description
);

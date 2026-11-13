using System;

namespace MeepleZone.Domaines.Collection.Contrats.Modeles;

public sealed record ModeJeuDto(
    Guid ModeJeuId,
    string Nom,
    string? Description
);

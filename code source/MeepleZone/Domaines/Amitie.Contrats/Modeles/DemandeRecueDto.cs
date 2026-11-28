using System;

namespace MeepleZone.Domaines.Amitie.Contrats.Modeles;

public sealed record DemandeRecueDto(
    Guid Id,
    string NomDemandeur,
    DateTimeOffset CreeLe
);

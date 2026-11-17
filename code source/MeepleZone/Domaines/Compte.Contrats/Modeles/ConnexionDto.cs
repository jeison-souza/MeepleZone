using System;

namespace MeepleZone.Domaines.Compte.Contrats.Modeles;

public sealed record ConnexionDto(
    string Fournisseur,
    string? Courriel,
    DateTimeOffset CreeLe
);

using System;

namespace MeepleZone.Domaines.Compte.Contrats.Modeles;

public sealed record UtilisateurConnecteDto(
    Guid Id,
    string NomAffichage,
    string? Courriel,
    string Role
);

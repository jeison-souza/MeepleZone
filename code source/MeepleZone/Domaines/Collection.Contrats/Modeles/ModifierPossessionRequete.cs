using System;

namespace MeepleZone.Domaines.Collection.Contrats.Modeles;

public sealed record ModifierPossessionRequete(
    Guid PossessionId,
    short? Note,
    string? Commentaire,
    int PartiesJouees
);

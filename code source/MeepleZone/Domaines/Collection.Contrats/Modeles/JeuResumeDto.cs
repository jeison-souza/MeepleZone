using System;

namespace MeepleZone.Domaines.Collection.Contrats.Modeles;

public sealed record JeuResumeDto(
    Guid JeuId,
    string Nom,
    short? AnneePublication,
    string? Editeur,
    bool DejaPossede
);

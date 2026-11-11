using System;

namespace MeepleZone.Domaines.Collection.Contrats.Modeles;

public sealed record ItemCollectionDto(
    Guid PossessionId,
    Guid JeuId,
    string Nom,
    short? AnneePublication,
    short? JoueursMin,
    short? JoueursMax,
    short? DureeMinMinutes,
    short? DureeMaxMinutes,
    Guid? CouvertureId,
    string? CouvertureUrl,
    short? Note,
    int PartiesJouees,
    string? Commentaire,
    string StatutJeu
);

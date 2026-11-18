using System;
using System.Collections.Generic;

namespace MeepleZone.Domaines.Collection.Contrats.Modeles;

public sealed record ItemCollectionDto(
    Guid PossessionId,
    Guid JeuId,
    short? AnneePublication,
    string? Commentaire,
    Guid? CouvertureId,
    string? CouvertureUrl,
    short? DureeMinMinutes,
    short? DureeMaxMinutes,
    short? JoueursMin,
    short? JoueursMax,
    short? JoueursMeilleur,
    IReadOnlyList<string> MecaniqueNoms,
    string? ModeJeuNom,
    string Nom,
    short? Note,
    int PartiesJouees,
    string StatutJeu
);

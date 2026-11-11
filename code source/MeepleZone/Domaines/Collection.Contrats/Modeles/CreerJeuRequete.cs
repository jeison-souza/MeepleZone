using System;
using System.Collections.Generic;

namespace MeepleZone.Domaines.Collection.Contrats.Modeles;

public sealed record CreerJeuRequete(
    string Nom,
    short? AnneePublication,
    short? JoueursMin,
    short? JoueursMax,
    short? JoueursMeilleur,
    short? DureeMinMinutes,
    short? DureeMaxMinutes,
    short? AgeMin,
    decimal? Complexite,
    Guid? ModeJeuId,
    string? Editeur,
    IReadOnlyList<Guid> MecaniqueIds
);

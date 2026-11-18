using System;
using System.Collections.Generic;

namespace MeepleZone.Domaines.Collection.Contrats.Modeles;

public sealed record FiltreCollection(
    short? NombreJoueurs,
    short? JoueursMeilleur,
    short? DureeMaxMinutes,
    short? NoteMin,
    int? PartiesJoueesMin,
    int? PartiesJoueesMax,
    Guid? ModeJeuId,
    IReadOnlyList<Guid>? MecaniqueIds
);

using System;
using System.Collections.Generic;

namespace MeepleZone.Systeme.Depots.Modeles;

/// <summary>
/// Critères optionnels de filtre sur la collection d'une personne. Un critère absent n'exclut rien ;
/// un critère présent exclut les jeux dont la valeur correspondante est inconnue.
/// </summary>
public sealed record FiltrePossessions(
    short? NombreJoueurs,
    short? JoueursMeilleur,
    short? DureeMaxMinutes,
    short? NoteMin,
    int? PartiesJoueesMin,
    int? PartiesJoueesMax,
    Guid? ModeJeuId,
    IReadOnlyList<Guid>? MecaniqueIds
);

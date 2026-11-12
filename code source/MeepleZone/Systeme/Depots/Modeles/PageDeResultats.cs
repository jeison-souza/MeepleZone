using System.Collections.Generic;

namespace MeepleZone.Systeme.Depots.Modeles;

public sealed record PageDeResultats<T>(IReadOnlyList<T> Items, int Total);

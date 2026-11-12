using System.Collections.Generic;

namespace MeepleZone.Systeme.Depots;

public sealed record PageDePossessions(IReadOnlyList<PossessionAvecJeu> Items, int Total);

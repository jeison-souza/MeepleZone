using System.Collections.Generic;

namespace MeepleZone.Domaines.Collection.Contrats.Modeles;

public sealed record RechercherJeuxReponse(
    IReadOnlyList<JeuResumeDto> Jeux,
    int Total
);

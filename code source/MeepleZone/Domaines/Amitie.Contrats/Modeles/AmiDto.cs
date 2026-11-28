using System;

namespace MeepleZone.Domaines.Amitie.Contrats.Modeles;

public sealed record AmiDto(
    Guid UtilisateurId,
    string NomAffichage,
    DateTimeOffset AmisDepuis,
    bool CollectionVisible
);

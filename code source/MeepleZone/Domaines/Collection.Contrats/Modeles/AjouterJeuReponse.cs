using MeepleZone.Domaines.Collection.Contrats.Enums;

namespace MeepleZone.Domaines.Collection.Contrats.Modeles;

public sealed record AjouterJeuReponse(
    StatutAjout Statut,
    ItemCollectionDto? Item
);

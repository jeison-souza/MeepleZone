namespace MeepleZone.Domaines.Compte.Contrats.Modeles;

public sealed record GererConnexionRequete(
    string Fournisseur,
    string IdentifiantCompteExterne,
    string? Courriel,
    string? NomAffichage,
    bool ForcerCreation
);

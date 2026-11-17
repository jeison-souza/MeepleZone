namespace MeepleZone.Domaines.Compte.Contrats.Modeles;

public sealed record LierFournisseurRequete(
    string Fournisseur,
    string IdentifiantCompteExterne,
    string? Courriel
);

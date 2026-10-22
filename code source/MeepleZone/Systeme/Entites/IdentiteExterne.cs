using System;

namespace MeepleZone.Systeme.Entites;

public class IdentiteExterne
{
    public Guid Id { get; set; }

    public Guid UtilisateurId { get; set; }

    public string Fournisseur { get; set; } = string.Empty;

    public string IdentifiantCompteExterne { get; set; } = string.Empty;

    public string? Courriel { get; set; }

    public DateTimeOffset DerniereConnexionLe { get; set; }

    public DateTimeOffset CreeLe { get; private set; }

    public DateTimeOffset ModifieLe { get; private set; }
}

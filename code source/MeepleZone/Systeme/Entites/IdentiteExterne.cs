using MeepleZone.Systeme.Entites.Attributs;
using MeepleZone.Systeme.Entites.Interfaces;
using System;

namespace MeepleZone.Systeme.Entites;

public class IdentiteExterne : IModifieLe
{
    public Guid Id { get; set; }

    public Guid UtilisateurId { get; set; }

    public string Fournisseur { get; set; } = string.Empty;

    public string IdentifiantCompteExterne { get; set; } = string.Empty;

    public string? Courriel { get; set; }

    [AttributTechnique]
    public DateTimeOffset DerniereConnexionLe { get; set; }

    public DateTimeOffset CreeLe { get; private set; }

    public DateTimeOffset ModifieLe { get; private set; }
}

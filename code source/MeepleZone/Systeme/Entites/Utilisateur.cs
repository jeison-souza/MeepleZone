using System;

namespace MeepleZone.Systeme.Entites;

public class Utilisateur
{
    public Guid Id { get; set; }

    public string? Courriel { get; set; }

    public DateTimeOffset? CourrielVerifieLe { get; set; }

    public string NomAffichage { get; set; } = string.Empty;

    public string Role { get; set; } = "membre";

    public Guid? PhotoId { get; set; }

    public string CodeAmitie { get; set; } = string.Empty;

    public string VisibiliteCollection { get; set; } = "privee";

    public DateTimeOffset CreeLe { get; set; }

    public DateTimeOffset ModifieLe { get; set; }
}

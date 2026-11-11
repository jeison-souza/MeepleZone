using System;

namespace MeepleZone.Systeme.Entites;

public class ModeJeu
{
    public Guid Id { get; set; }

    public string Nom { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTimeOffset CreeLe { get; private set; }
}

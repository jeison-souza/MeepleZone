using System;

namespace MeepleZone.Systeme.Entites;

public class Mecanique
{
    public Guid Id { get; set; }

    public string Nom { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int? BggId { get; set; }

    public DateTimeOffset CreeLe { get; private set; }
}

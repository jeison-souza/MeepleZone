using System;

namespace MeepleZone.Systeme.Entites;

public class Amitie
{
    public Guid UtilisateurAId { get; set; }

    public Guid UtilisateurBId { get; set; }

    public DateTimeOffset CreeLe { get; private set; }
}

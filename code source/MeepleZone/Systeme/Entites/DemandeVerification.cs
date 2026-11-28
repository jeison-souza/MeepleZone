using System;

namespace MeepleZone.Systeme.Entites;

public class DemandeVerification
{
    public Guid Id { get; set; }

    public Guid UtilisateurId { get; set; }

    public string Courriel { get; set; } = string.Empty;

    public DateTimeOffset CreeLe { get; private set; }

    public DateTimeOffset? EnvoyeLe { get; set; }

    public TimeSpan DureeValidite { get; set; }
}

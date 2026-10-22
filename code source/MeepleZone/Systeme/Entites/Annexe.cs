using System;

namespace MeepleZone.Systeme.Entites;

public class Annexe
{
    public Guid Id { get; set; }

    public string NomFichier { get; set; } = string.Empty;

    public byte[] Contenu { get; set; } = Array.Empty<byte>();

    public string TypeMime { get; set; } = string.Empty;

    public int TailleOctets { get; set; } = 0;

    public Guid CreeParId { get; set; }

    public DateTimeOffset CreeLe { get; private set; }
}

using MeepleZone.Systeme.Entites.Interfaces;
using System;

namespace MeepleZone.Systeme.Entites;

public class Possession : IModifieLe
{
    public Guid Id { get; set; }

    public Guid UtilisateurId { get; set; }

    public Guid JeuId { get; set; }

    public short? Note { get; set; }

    public string? Commentaire { get; set; }

    public int PartiesJouees { get; set; }

    public DateTimeOffset CreeLe { get; private set; }

    public DateTimeOffset ModifieLe { get; private set; }
}

using MeepleZone.Systeme.Entites.Interfaces;
using System;

namespace MeepleZone.Systeme.Entites;

public class Jeu : IModifieLe
{
    public Guid Id { get; set; }

    public string Nom { get; set; } = string.Empty;

    public short? AnneePublication { get; set; }

    public short? JoueursMin { get; set; }

    public short? JoueursMax { get; set; }

    public short? JoueursMeilleur { get; set; }

    public short? DureeMinMinutes { get; set; }

    public short? DureeMaxMinutes { get; set; }

    public short? AgeMin { get; set; }

    public decimal? Complexite { get; set; }

    public Guid? ModeJeuId { get; set; }

    public string? Editeur { get; set; }

    public int? BggId { get; set; }

    public Guid? CouvertureId { get; set; }

    public string? CouvertureUrl { get; set; }

    public string Statut { get; set; } = "en_attente";

    public Guid CreeParId { get; set; }

    public DateTimeOffset CreeLe { get; private set; }

    public DateTimeOffset ModifieLe { get; private set; }
}

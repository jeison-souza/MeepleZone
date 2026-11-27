using MeepleZone.Systeme.Entites.Constantes;
using MeepleZone.Systeme.Entites.Interfaces;
using System;

namespace MeepleZone.Systeme.Entites;

public class DemandeAmitie : IModifieLe
{
    public Guid Id { get; set; }

    public Guid DemandeurId { get; set; }

    public Guid? DestinataireId { get; set; }

    public string? CourrielDestinataire { get; set; }

    public string Statut { get; set; } = StatutsDemandeAmitie.EnAttente;

    public DateTimeOffset CreeLe { get; private set; }

    public DateTimeOffset ModifieLe { get; private set; }
}

using MeepleZone.Domaines.Compte.Contrats.Enums;

namespace MeepleZone.Domaines.Compte.Contrats.Modeles;

public sealed record RegenererCodeAmitieReponse(StatutRegenerationCode Statut, string? CodeAmitie);

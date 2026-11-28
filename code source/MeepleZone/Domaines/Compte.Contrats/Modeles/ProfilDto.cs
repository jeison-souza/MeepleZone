using MeepleZone.Domaines.Compte.Contrats.Enums;

namespace MeepleZone.Domaines.Compte.Contrats.Modeles;

public sealed record ProfilDto(string NomAffichage, string? Courriel, string CodeAmitie, VisibiliteCollection VisibiliteCollection);

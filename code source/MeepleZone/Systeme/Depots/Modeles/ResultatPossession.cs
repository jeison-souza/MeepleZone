using MeepleZone.Systeme.Entites;
using System.Collections.Generic;

namespace MeepleZone.Systeme.Depots.Modeles;

/// <summary>
/// Possession avec son jeu chargé, le nom du mode de jeu et les noms des mécaniques (triés).
/// </summary>
public sealed record ResultatPossession(Possession Possession, string? ModeJeuNom, IReadOnlyList<string> MecaniqueNoms);

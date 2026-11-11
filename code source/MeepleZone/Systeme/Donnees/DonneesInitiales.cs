using System;

namespace MeepleZone.Systeme.Donnees;

// Les données de référence sont insérées par les migrations (HasData). Les valeurs, dont les
// identifiants, doivent être fixes : la même date de création sert à toutes les lignes.
internal static class DonneesInitiales
{
    public static readonly DateTimeOffset CreeLe = new(2026, 11, 11, 0, 0, 0, TimeSpan.Zero);
}

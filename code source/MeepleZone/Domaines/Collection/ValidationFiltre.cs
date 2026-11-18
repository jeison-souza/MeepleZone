using MeepleZone.Domaines.Collection.Contrats.Modeles;
using MeepleZone.Systeme.Depots.Modeles;
using System;
using System.Linq;

namespace MeepleZone.Domaines.Collection;

public static class ValidationFiltre
{
    public const short NoteMinimale = 1;

    public const short NoteMaximale = 10;

    /// <summary>
    /// Valide le filtre (mêmes bornes que les contraintes du schéma) et le convertit pour le dépôt.
    /// Une valeur hors limites est une erreur de programmation de l'appelant, comme pour la pagination.
    /// </summary>
    public static FiltrePossessions? Convertir(FiltreCollection? filtre)
    {
        if (filtre == null)
        {
            return null;
        }

        ExigerAuMoins(filtre.NombreJoueurs, 1, nameof(filtre.NombreJoueurs));
        ExigerAuMoins(filtre.JoueursMeilleur, 1, nameof(filtre.JoueursMeilleur));
        ExigerAuMoins(filtre.DureeMaxMinutes, 1, nameof(filtre.DureeMaxMinutes));
        ExigerAuMoins(filtre.PartiesJoueesMin, 0, nameof(filtre.PartiesJoueesMin));
        ExigerAuMoins(filtre.PartiesJoueesMax, 0, nameof(filtre.PartiesJoueesMax));

        if (filtre.NoteMin is { } note && (note < NoteMinimale || note > NoteMaximale))
        {
            throw new ArgumentOutOfRangeException(nameof(filtre.NoteMin), note, $"La note minimale doit être comprise entre {NoteMinimale} et {NoteMaximale}.");
        }

        if (filtre.PartiesJoueesMin is { } min && filtre.PartiesJoueesMax is { } max && min > max)
        {
            throw new ArgumentOutOfRangeException(nameof(filtre.PartiesJoueesMin), min, "Le minimum de parties jouées ne peut pas dépasser le maximum.");
        }

        var mecaniqueIds = filtre.MecaniqueIds?.Distinct().ToList();

        return new FiltrePossessions(
            filtre.NombreJoueurs,
            filtre.JoueursMeilleur,
            filtre.DureeMaxMinutes,
            filtre.NoteMin,
            filtre.PartiesJoueesMin,
            filtre.PartiesJoueesMax,
            filtre.ModeJeuId,
            mecaniqueIds);
    }

    private static void ExigerAuMoins(int? valeur, int minimum, string nom)
    {
        if (valeur is { } v && v < minimum)
        {
            throw new ArgumentOutOfRangeException(nom, v, $"La valeur doit être au moins {minimum}.");
        }
    }
}

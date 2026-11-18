using MeepleZone.Domaines.Collection.Contrats.Modeles;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MeepleZone.Site.Collection;

/// <summary>
/// Filtres de la page « Ma collection » : valeurs choisies, conversion pour le domaine (qui applique les règles)
/// et lien avec l'adresse de la page.
/// </summary>
public sealed record FiltresCollection
{
    public const string CleJoueurs = "joueurs";
    public const string CleOptimal = "optimal";
    public const string CleDuree = "duree";
    public const string CleNote = "note";
    public const string CleParties = "parties";
    public const string CleMode = "mode";
    public const string CleMecaniques = "mecaniques";
    public const string ClePartiesMin = "parties-min";
    public const string ClePartiesMax = "parties-max";

    /// <summary>Nombre de joueurs : le jeu convient si min ≤ N ≤ max.</summary>
    public short? NombreJoueurs { get; init; }

    /// <summary>Avec un nombre de joueurs : seulement les jeux dont c'est le meilleur nombre.</summary>
    public bool Optimal { get; init; }

    /// <summary>Durée « au plus » (minutes).</summary>
    public short? DureeMax { get; init; }

    /// <summary>Note minimale : les jeux sans note sont exclus.</summary>
    public short? NoteMin { get; init; }

    public int? PartiesMin { get; init; }

    /// <summary>Un maximum de 0 veut dire « jamais joué ».</summary>
    public int? PartiesMax { get; init; }

    public Guid? ModeJeuId { get; init; }

    /// <summary>Le jeu doit avoir au moins une des mécaniques choisies.</summary>
    public IReadOnlyList<Guid> MecaniqueIds { get; init; } = [];

    public bool EstActif => NombreJoueurs is not null
        || DureeMax is not null
        || NoteMin is not null
        || PartiesMin is not null
        || PartiesMax is not null
        || ModeJeuId is not null
        || MecaniqueIds.Count > 0;

    /// <summary>Nombre de critères actifs (« Optimal » compte avec le nombre de joueurs).</summary>
    public int NombreCriteres => Chips(new Dictionary<Guid, string>(), new Dictionary<Guid, string>()).Count;

    /// <summary>Change le minimum de parties; le maximum suit s'il devient inférieur (le domaine refuse min &gt; max).</summary>
    public FiltresCollection AvecPartiesMin(int? minimum)
    {
        return this with { PartiesMin = minimum, PartiesMax = minimum is { } m && PartiesMax < m ? m : PartiesMax };
    }

    /// <summary>Change le maximum de parties; le minimum suit s'il devient supérieur.</summary>
    public FiltresCollection AvecPartiesMax(int? maximum)
    {
        return this with { PartiesMax = maximum, PartiesMin = maximum is { } m && PartiesMin > m ? m : PartiesMin };
    }

    /// <summary>
    /// Filtre du domaine, ou null sans critère. Avec « Idéal », seul le meilleur nombre de joueurs est transmis
    /// (il implique que le jeu convient); sinon le jeu doit pouvoir se jouer à N. Le domaine exclut les jeux sans la donnée visée.
    /// </summary>
    public FiltreCollection? VersContrat()
    {
        if (!EstActif)
        {
            return null;
        }

        return new FiltreCollection(
            NombreJoueurs: Optimal ? null : NombreJoueurs,
            JoueursMeilleur: Optimal ? NombreJoueurs : null,
            DureeMaxMinutes: DureeMax,
            NoteMin: NoteMin,
            PartiesJoueesMin: PartiesMin,
            PartiesJoueesMax: PartiesMax,
            ModeJeuId: ModeJeuId,
            MecaniqueIds: MecaniqueIds.Count > 0 ? MecaniqueIds : null);
    }

    /// <summary>Paramètres d'adresse des filtres actifs, sans les valeurs vides.</summary>
    public IReadOnlyDictionary<string, string> VersParametres()
    {
        var parametres = new Dictionary<string, string>();

        if (NombreJoueurs is { } joueurs)
        {
            parametres[CleJoueurs] = Texte(joueurs);

            if (Optimal)
            {
                parametres[CleOptimal] = "1";
            }
        }

        Ajouter(parametres, CleDuree, DureeMax);
        Ajouter(parametres, CleNote, NoteMin);
        Ajouter(parametres, ClePartiesMin, PartiesMin);
        Ajouter(parametres, ClePartiesMax, PartiesMax);

        if (ModeJeuId is { } mode)
        {
            parametres[CleMode] = mode.ToString("D");
        }

        if (MecaniqueIds.Count > 0)
        {
            parametres[CleMecaniques] = string.Join(',', MecaniqueIds.Select(id => id.ToString("D")));
        }

        return parametres;
    }

    /// <summary>Lit les filtres d'une adresse; une valeur invalide ou hors limites est ignorée.</summary>
    public static FiltresCollection DepuisParametres(
        string? joueurs, string? optimal, string? duree, string? note,
        string? partiesMin, string? partiesMax, string? mode, string? mecaniques)
    {
        var nombreJoueurs = Entier(joueurs, 1, 99);

        var filtres = new FiltresCollection
        {
            NombreJoueurs = nombreJoueurs is null ? null : (short)nombreJoueurs,
            Optimal = nombreJoueurs is not null && optimal == "1",
            DureeMax = EnShort(Entier(duree, 1, 1440)),
            NoteMin = EnShort(Entier(note, 1, 10)),
            PartiesMin = Entier(partiesMin, 0, 100000),
            ModeJeuId = Guid.TryParse(mode, out var modeId) ? modeId : null,
            MecaniqueIds = LireIdentifiants(mecaniques),
        };

        return filtres.AvecPartiesMax(Entier(partiesMax, 0, 100000));
    }

    /// <summary>Même contenu (les listes sont comparées par leurs éléments, et non par référence).</summary>
    public bool EstIdentique(FiltresCollection autre)
    {
        return VersParametres().OrderBy(p => p.Key).SequenceEqual(autre.VersParametres().OrderBy(p => p.Key));
    }

    /// <summary>Un chip par critère actif, avec la clé qui sert à le retirer.</summary>
    public IReadOnlyList<ChipFiltre> Chips(IReadOnlyDictionary<Guid, string> modes, IReadOnlyDictionary<Guid, string> mecaniques)
    {
        var chips = new List<ChipFiltre>();

        if (NombreJoueurs is { } n)
        {
            chips.Add(new ChipFiltre(CleJoueurs, Optimal ? $"Idéal à {n} joueurs" : n == 1 ? "1 joueur" : $"{n} joueurs"));
        }

        if (DureeMax is { } duree)
        {
            chips.Add(new ChipFiltre(CleDuree, $"{duree} min ou moins"));
        }

        if (NoteMin is { } note)
        {
            chips.Add(new ChipFiltre(CleNote, $"Note d'au moins {note}"));
        }

        if (LibelleParties() is { } parties)
        {
            chips.Add(new ChipFiltre(CleParties, parties));
        }

        if (ModeJeuId is { } mode)
        {
            chips.Add(new ChipFiltre(CleMode, modes.GetValueOrDefault(mode, "Mode de jeu")));
        }

        foreach (var id in MecaniqueIds)
        {
            chips.Add(new ChipFiltre($"{CleMecaniques}:{id:D}", mecaniques.GetValueOrDefault(id, "Mécanique")));
        }

        return chips;
    }

    /// <summary>Retire le critère désigné par la clé d'un chip.</summary>
    public FiltresCollection Sans(string cle)
    {
        if (cle.StartsWith(CleMecaniques + ":", StringComparison.Ordinal)
            && Guid.TryParse(cle[(CleMecaniques.Length + 1)..], out var identifiant))
        {
            return this with { MecaniqueIds = MecaniqueIds.Where(id => id != identifiant).ToList() };
        }

        return cle switch
        {
            CleJoueurs => this with { NombreJoueurs = null, Optimal = false },
            CleDuree => this with { DureeMax = null },
            CleNote => this with { NoteMin = null },
            CleParties => this with { PartiesMin = null, PartiesMax = null },
            CleMode => this with { ModeJeuId = null },
            _ => this,
        };
    }

    private string? LibelleParties()
    {
        return (PartiesMin, PartiesMax) switch
        {
            (null, null) => null,
            (null, 0) or (0, 0) => "Jamais joué",
            (null, { } max) => $"{max} parties jouées ou moins",
            ({ } min, null) => $"{min} parties jouées ou plus",
            ({ } min, { } max) when min == max => $"{min} parties jouées",
            ({ } min, { } max) => $"{min} à {max} parties jouées",
        };
    }

    private static void Ajouter(Dictionary<string, string> parametres, string cle, int? valeur)
    {
        if (valeur is { } v)
        {
            parametres[cle] = Texte(v);
        }
    }

    private static string Texte(int valeur) => valeur.ToString(CultureInfo.InvariantCulture);

    private static int? Entier(string? texte, int minimum, int maximum)
    {
        return int.TryParse(texte, NumberStyles.None, CultureInfo.InvariantCulture, out var valeur)
            && valeur >= minimum && valeur <= maximum
            ? valeur
            : null;
    }

    private static short? EnShort(int? valeur) => valeur is { } v ? (short)v : null;

    private static List<Guid> LireIdentifiants(string? texte)
    {
        if (string.IsNullOrWhiteSpace(texte))
        {
            return [];
        }

        return texte.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(partie => Guid.TryParse(partie, out var id) ? id : (Guid?)null)
            .OfType<Guid>()
            .Distinct()
            .ToList();
    }
}

/// <summary>Un critère actif affiché comme chip; <see cref="Cle"/> sert à le retirer avec <see cref="FiltresCollection.Sans"/>.</summary>
public sealed record ChipFiltre(string Cle, string Libelle);

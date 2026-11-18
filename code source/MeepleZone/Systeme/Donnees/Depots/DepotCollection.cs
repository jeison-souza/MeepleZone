using MeepleZone.Systeme.Depots;
using MeepleZone.Systeme.Depots.Enums;
using MeepleZone.Systeme.Depots.Modeles;
using MeepleZone.Systeme.Entites;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeepleZone.Systeme.Donnees.Depots;

public class DepotCollection(IDbContextFactory<MeepleZoneContexte> fabrique) : IDepotCollection
{
    /// <summary>
    /// Sous ce nombre de caractères, un texte produit trop peu de trigrammes pour une similarité utile (ADR 0015).
    /// </summary>
    private const int LongueurMinimaleSimilarite = 3;

    /// <summary>
    /// Seuil de la similarité par mots de la recherche : privilégie le rappel (retrouver largement). Valeur à régler sur des données réelles.
    /// </summary>
    private const double SeuilSimilariteMots = 0.6;

    /// <summary>
    /// Seuil de la similarité du nom entier pour les doublons : privilégie la précision (ne pas accuser à tort). Valeur à régler sur des données réelles.
    /// </summary>
    private const double SeuilSimilariteDoublons = 0.5;

    private const string StatutValide = "valide";

    private const string StatutEnAttente = "en_attente";

    private const string IndexPossessionUnique = "ix_possession_utilisateur_id_jeu_id";

    private const string CleEtrangerePossessionJeu = "fk_possession_jeu_jeu_id";

    private const string CleEtrangereJeuModeJeu = "fk_jeu_mode_jeu_mode_jeu_id";

    private const string CleEtrangereJeuMecaniqueMecanique = "fk_jeu_mecanique_mecanique_mecanique_id";

    private readonly IDbContextFactory<MeepleZoneContexte> _fabrique = fabrique;

    public async Task<PageDeResultats<ResultatPossession>> ListerPossessionsAsync(Guid utilisateurId, string? recherche, FiltrePossessions? filtre, int saut, int prendre)
    {
        await using var contexte = await _fabrique.CreateDbContextAsync();

        var possessions = contexte.Possessions
            .AsNoTracking()
            .Where(p => p.UtilisateurId == utilisateurId);

        if (!string.IsNullOrWhiteSpace(recherche))
        {
            possessions = possessions.Where(p => EF.Functions.Unaccent(p.Jeu!.Nom).ToLower().Contains(EF.Functions.Unaccent(recherche).ToLower()));
        }

        if (filtre != null)
        {
            possessions = AppliquerFiltre(contexte, possessions, filtre);
        }

        var total = await possessions.CountAsync();

        var items = await possessions
            .OrderBy(p => p.Jeu!.Nom)
            .ThenBy(p => p.Id)
            .Skip(saut)
            .Take(prendre)
            .Select(p => new
            {
                Possession = p,
                p.Jeu,
                ModeJeuNom = contexte.ModesJeu
                    .Where(m => m.Id == p.Jeu!.ModeJeuId)
                    .Select(m => m.Nom)
                    .FirstOrDefault(),
                MecaniqueNoms = contexte.JeuxMecaniques
                    .Where(jm => jm.JeuId == p.JeuId)
                    .Join(contexte.Mecaniques, jm => jm.MecaniqueId, m => m.Id, (jm, m) => m.Nom)
                    .OrderBy(nom => nom)
                    .ToList()
            })
            .ToListAsync();

        // La requête en lecture seule ne rattache pas le jeu à la possession : le rattachement se fait ici.
        return new PageDeResultats<ResultatPossession>(
            [.. items.Select(x =>
            {
                x.Possession.Jeu = x.Jeu;
                return new ResultatPossession(x.Possession, x.ModeJeuNom, x.MecaniqueNoms);
            })],
            total);
    }

    private static IQueryable<Possession> AppliquerFiltre(MeepleZoneContexte contexte, IQueryable<Possession> possessions, FiltrePossessions filtre)
    {
        if (filtre.NombreJoueurs is { } nombre)
        {
            possessions = possessions.Where(p => p.Jeu!.JoueursMin <= nombre && p.Jeu.JoueursMax >= nombre);
        }

        if (filtre.JoueursMeilleur is { } meilleur)
        {
            possessions = possessions.Where(p => p.Jeu!.JoueursMeilleur == meilleur);
        }

        if (filtre.DureeMaxMinutes is { } duree)
        {
            possessions = possessions.Where(p => p.Jeu!.DureeMaxMinutes <= duree);
        }

        if (filtre.NoteMin is { } noteMin)
        {
            possessions = possessions.Where(p => p.Note >= noteMin);
        }

        if (filtre.PartiesJoueesMin is { } partiesMin)
        {
            possessions = possessions.Where(p => p.PartiesJouees >= partiesMin);
        }

        if (filtre.PartiesJoueesMax is { } partiesMax)
        {
            possessions = possessions.Where(p => p.PartiesJouees <= partiesMax);
        }

        if (filtre.ModeJeuId is { } modeJeuId)
        {
            possessions = possessions.Where(p => p.Jeu!.ModeJeuId == modeJeuId);
        }

        if (filtre.MecaniqueIds is { Count: > 0 } mecaniqueIds)
        {
            var ids = mecaniqueIds.ToList();
            possessions = possessions.Where(p => contexte.JeuxMecaniques.Any(jm => jm.JeuId == p.JeuId && ids.Contains(jm.MecaniqueId)));
        }

        return possessions;
    }

    public async Task<Possession?> ModifierPossessionAsync(Guid utilisateurId, Guid possessionId, short? note, string? commentaire, int partiesJouees)
    {
        await using var contexte = await _fabrique.CreateDbContextAsync();

        var possession = await contexte.Possessions
            .Include(p => p.Jeu)
            .SingleOrDefaultAsync(p => p.UtilisateurId == utilisateurId &&
                                       p.Id == possessionId);

        if (possession == null)
        {
            return null;
        }

        possession.Note = note;
        possession.Commentaire = commentaire;
        possession.PartiesJouees = partiesJouees;

        await contexte.SaveChangesAsync();
        return possession;
    }

    public async Task<PageDeResultats<ResultatRechercheJeu>> RechercherJeuxAsync(Guid utilisateurId, string texte, int saut, int prendre)
    {
        await using var contexte = await _fabrique.CreateDbContextAsync();

        var visibles = contexte.Jeux
            .AsNoTracking()
            .Where(j => j.Statut == StatutValide || (j.Statut == StatutEnAttente && j.CreeParId == utilisateurId));

        // Le texte est un paramètre, jamais un motif : Contains devient strpos, les jokers restent des caractères.
        // Un texte trop court produit trop peu de trigrammes : la recherche se limite alors à « contient ».
        IQueryable<Jeu> jeux;
        IOrderedQueryable<Jeu> ordonnes;
        if (texte.Length >= LongueurMinimaleSimilarite)
        {
            jeux = visibles.Where(j =>
                j.NomRecherche.Contains(EF.Functions.Unaccent(texte).ToLower()) ||
                EF.Functions.TrigramsWordSimilarity(EF.Functions.Unaccent(texte).ToLower(), j.NomRecherche) >= SeuilSimilariteMots);

            ordonnes = jeux
                .OrderBy(j => j.NomRecherche.Contains(EF.Functions.Unaccent(texte).ToLower()) ? 0 : 1)
                .ThenByDescending(j => EF.Functions.TrigramsWordSimilarity(EF.Functions.Unaccent(texte).ToLower(), j.NomRecherche))
                .ThenBy(j => j.Nom);
        }
        else
        {
            jeux = visibles.Where(j => j.NomRecherche.Contains(EF.Functions.Unaccent(texte).ToLower()));
            ordonnes = jeux.OrderBy(j => j.Nom);
        }

        var total = await jeux.CountAsync();

        // L'identifiant départage les noms identiques : la pagination reste stable.
        var items = await ordonnes
            .ThenBy(j => j.Id)
            .Skip(saut)
            .Take(prendre)
            .Select(j => new
            {
                Jeu = j,
                DejaPossede = contexte.Possessions.Any(p => p.UtilisateurId == utilisateurId && p.JeuId == j.Id)
            })
            .ToListAsync();

        return new PageDeResultats<ResultatRechercheJeu>([.. items.Select(x => new ResultatRechercheJeu(x.Jeu, x.DejaPossede))], total);
    }

    public async Task<IReadOnlyList<ResultatRechercheJeu>> DetecterDoublonsAsync(Guid utilisateurId, string nom, int limite)
    {
        await using var contexte = await _fabrique.CreateDbContextAsync();

        var visibles = contexte.Jeux
            .AsNoTracking()
            .Where(j => j.Statut == StatutValide || (j.Statut == StatutEnAttente && j.CreeParId == utilisateurId));

        // Similarité du nom entier (précision) : plus stricte que la recherche par mots (rappel).
        // Un nom trop court produit trop peu de trigrammes : seul un nom identique (sans accents ni casse) est un doublon.
        IQueryable<Jeu> candidats;
        if (nom.Length >= LongueurMinimaleSimilarite)
        {
            candidats = visibles
                .Where(j => EF.Functions.TrigramsSimilarity(EF.Functions.Unaccent(nom).ToLower(), j.NomRecherche) >= SeuilSimilariteDoublons)
                .OrderByDescending(j => EF.Functions.TrigramsSimilarity(EF.Functions.Unaccent(nom).ToLower(), j.NomRecherche))
                .ThenBy(j => j.Nom)
                .ThenBy(j => j.Id);
        }
        else
        {
            candidats = visibles
                .Where(j => j.NomRecherche == EF.Functions.Unaccent(nom).ToLower())
                .OrderBy(j => j.Nom)
                .ThenBy(j => j.Id);
        }

        var items = await candidats
            .Take(limite)
            .Select(j => new
            {
                Jeu = j,
                DejaPossede = contexte.Possessions.Any(p => p.UtilisateurId == utilisateurId && p.JeuId == j.Id)
            })
            .ToListAsync();

        return [.. items.Select(x => new ResultatRechercheJeu(x.Jeu, x.DejaPossede))];
    }

    public async Task<bool> RetirerPossessionAsync(Guid utilisateurId, Guid possessionId)
    {
        await using var contexte = await _fabrique.CreateDbContextAsync();

        return 0 < await contexte.Possessions
            .Where(p => p.UtilisateurId == utilisateurId &&
                        p.Id == possessionId)
            .ExecuteDeleteAsync();
    }

    public async Task<Jeu?> TrouverJeuVisibleAsync(Guid utilisateurId, Guid jeuId)
    {
        await using var contexte = await _fabrique.CreateDbContextAsync();

        return await contexte.Jeux
            .AsNoTracking()
            .Where(j => j.Id == jeuId &&
                        (j.Statut == StatutValide || (j.Statut == StatutEnAttente && j.CreeParId == utilisateurId)))
            .SingleOrDefaultAsync();
    }

    public async Task<ResultatAjoutPossession> AjouterPossessionAsync(Possession possession)
    {
        await using var contexte = await _fabrique.CreateDbContextAsync();

        try
        {
            contexte.Possessions.Add(possession);
            await contexte.SaveChangesAsync();
            return ResultatAjoutPossession.Ajoutee;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: IndexPossessionUnique })
        {
            return ResultatAjoutPossession.DejaPossedee;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.ForeignKeyViolation, ConstraintName: CleEtrangerePossessionJeu })
        {
            return ResultatAjoutPossession.JeuInexistant;
        }
    }

    public async Task<ResultatCreationJeu> CreerJeuAsync(Jeu jeu, IReadOnlyList<Guid> mecaniqueIds, Possession possession)
    {
        await using var contexte = await _fabrique.CreateDbContextAsync();

        try
        {
            contexte.Jeux.Add(jeu);
            contexte.JeuxMecaniques.AddRange(mecaniqueIds.Select(id => new JeuMecanique { JeuId = jeu.Id, MecaniqueId = id }));
            contexte.Possessions.Add(possession);
            await contexte.SaveChangesAsync();
            return ResultatCreationJeu.Cree;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.ForeignKeyViolation, ConstraintName: CleEtrangereJeuModeJeu or CleEtrangereJeuMecaniqueMecanique })
        {
            return ResultatCreationJeu.ReferenceInvalide;
        }
    }
}

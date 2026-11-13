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
    private const string StatutValide = "valide";

    private const string StatutEnAttente = "en_attente";

    private const string IndexPossessionUnique = "ix_possession_utilisateur_id_jeu_id";

    private const string CleEtrangerePossessionJeu = "fk_possession_jeu_jeu_id";

    private const string CleEtrangereJeuModeJeu = "fk_jeu_mode_jeu_mode_jeu_id";

    private const string CleEtrangereJeuMecaniqueMecanique = "fk_jeu_mecanique_mecanique_mecanique_id";

    private readonly IDbContextFactory<MeepleZoneContexte> _fabrique = fabrique;

    public async Task<PageDeResultats<Possession>> ListerPossessionsAsync(Guid utilisateurId, string? recherche, int saut, int prendre)
    {
        await using var contexte = await _fabrique.CreateDbContextAsync();

        var possessions = contexte.Possessions
            .AsNoTracking()
            .Include(p => p.Jeu)
            .Where(p => p.UtilisateurId == utilisateurId);

        if (!string.IsNullOrWhiteSpace(recherche))
        {
            possessions = possessions.Where(p => EF.Functions.Unaccent(p.Jeu!.Nom).ToLower().Contains(EF.Functions.Unaccent(recherche).ToLower()));
        }

        var total = await possessions.CountAsync();

        var items = await possessions
            .OrderBy(p => p.Jeu!.Nom)
            .ThenBy(p => p.Id)
            .Skip(saut)
            .Take(prendre)
            .ToListAsync();

        return new PageDeResultats<Possession>(items, total);
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

        var jeux = contexte.Jeux
            .AsNoTracking()
            .Where(j => j.Statut == StatutValide || (j.Statut == StatutEnAttente && j.CreeParId == utilisateurId))
            .Where(j => EF.Functions.Unaccent(j.Nom).ToLower().Contains(EF.Functions.Unaccent(texte).ToLower()));

        var total = await jeux.CountAsync();

        var items = await jeux
            .OrderBy(j => j.Nom)
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

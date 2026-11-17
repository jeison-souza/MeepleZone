using MeepleZone.Systeme.Depots;
using MeepleZone.Systeme.Depots.Enums;
using MeepleZone.Systeme.Entites;
using MeepleZone.Systeme.Entites.Constantes;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeepleZone.Systeme.Donnees.Depots;

public class DepotCompte(IDbContextFactory<MeepleZoneContexte> fabrique) : IDepotCompte
{
    private const string CleEtrangerePossessionJeu = "fk_possession_jeu_jeu_id";

    private const string IndexIdentiteExterneUnique = "ix_identite_externe_fournisseur_identifiant_compte_externe";

    private const string IndexUtilisateurCourriel = "ix_utilisateur_courriel";

    private const string StatutJeuValide = "valide";

    private readonly IDbContextFactory<MeepleZoneContexte> _fabrique = fabrique;

    public async Task<ResultatAjoutIdentite> AjouterIdentiteAsync(IdentiteExterne identite)
    {
        await using var contexte = await _fabrique.CreateDbContextAsync();
        try
        {
            contexte.IdentitesExternes.Add(identite);
            await contexte.SaveChangesAsync();
            return ResultatAjoutIdentite.Ajoutee;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } violation)
        {
            switch (violation.ConstraintName)
            {
                case IndexIdentiteExterneUnique:
                    return ResultatAjoutIdentite.IdentiteDejaExistante;

                default:
                    throw;
            }
        }
    }

    public async Task<ResultatEnregistrementCompte> EnregistrerNouveauCompteAsync(Utilisateur utilisateur, IdentiteExterne identite)
    {
        await using var contexte = await _fabrique.CreateDbContextAsync();

        try
        {
            contexte.Utilisateurs.Add(utilisateur);
            contexte.IdentitesExternes.Add(identite);
            await contexte.SaveChangesAsync();
            return ResultatEnregistrementCompte.Enregistre;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } violation)
        {
            switch (violation.ConstraintName)
            {
                case IndexIdentiteExterneUnique:
                    return ResultatEnregistrementCompte.IdentiteDejaExistante;

                case IndexUtilisateurCourriel:
                    return ResultatEnregistrementCompte.CourrielDejaUtilise;

                default:
                    throw;
            }
        }
    }

    public async Task<IReadOnlyList<IdentiteExterne>> ListerIdentitesAsync(Guid utilisateurId)
    {
        await using var contexte = await _fabrique.CreateDbContextAsync();

        return await contexte.IdentitesExternes
            .AsNoTracking()
            .Where(i => i.UtilisateurId == utilisateurId)
            .OrderBy(i => i.CreeLe)
            .ThenBy(i => i.Id)
            .ToListAsync();
    }

    public async Task MettreAJourConnexionAsync(IdentiteExterne identite)
    {
        await using var contexte = await _fabrique.CreateDbContextAsync();

        contexte.IdentitesExternes.Update(identite);
        await contexte.SaveChangesAsync();
    }

    public async Task<ResultatSuppressionCompte> SupprimerCompteAsync(Guid utilisateurId, Guid compteSupprimeId)
    {
        await using var contexte = await _fabrique.CreateDbContextAsync();
        await using var transaction = await contexte.Database.BeginTransactionAsync();

        try
        {
            var compte = await contexte.Utilisateurs
                .AsNoTracking()
                .Where(u => u.Id == utilisateurId)
                .Select(u => new { u.Role, u.PhotoId })
                .FirstOrDefaultAsync();

            if (compte == null)
            {
                return ResultatSuppressionCompte.Introuvable;
            }

            if (Roles.EstProtege(compte.Role))
            {
                return ResultatSuppressionCompte.CompteProtege;
            }

            await contexte.Possessions
                .Where(p => p.UtilisateurId == utilisateurId)
                .ExecuteDeleteAsync();

            var jeuxASupprimer = await contexte.Jeux
                .AsNoTracking()
                .Where(j => j.CreeParId == utilisateurId
                    && j.Statut != StatutJeuValide
                    && !contexte.Possessions.Any(p => p.JeuId == j.Id))
                .Select(j => new { j.Id, j.CouvertureId })
                .ToListAsync();

            if (jeuxASupprimer.Count > 0)
            {
                var jeuxIds = jeuxASupprimer.Select(j => j.Id).ToList();
                var couverturesIds = jeuxASupprimer
                    .Where(j => j.CouvertureId != null)
                    .Select(j => j.CouvertureId!.Value)
                    .ToList();

                await contexte.Jeux.Where(j => jeuxIds.Contains(j.Id)).ExecuteDeleteAsync();
                await contexte.Annexes.Where(a => couverturesIds.Contains(a.Id)).ExecuteDeleteAsync();
            }

            if (compte.PhotoId != null)
            {
                await contexte.Annexes.Where(a => a.Id == compte.PhotoId).ExecuteDeleteAsync();
            }

            var jeuxRestants = await contexte.Jeux
                .Where(j => j.CreeParId == utilisateurId)
                .ToListAsync();

            foreach (var jeu in jeuxRestants)
            {
                jeu.CreeParId = compteSupprimeId;
            }

            // Annexes : seul l'identifiant est lu, pour ne pas charger le contenu des fichiers.
            var annexesRestantesIds = await contexte.Annexes
                .AsNoTracking()
                .Where(a => a.CreeParId == utilisateurId)
                .Select(a => a.Id)
                .ToListAsync();

            foreach (var annexeId in annexesRestantesIds)
            {
                var annexe = new Annexe { Id = annexeId };
                contexte.Annexes.Attach(annexe);
                annexe.CreeParId = compteSupprimeId;
            }

            await contexte.SaveChangesAsync();

            var supprimes = await contexte.Utilisateurs
                .Where(u => u.Id == utilisateurId)
                .ExecuteDeleteAsync();

            if (supprimes == 0)
            {
                return ResultatSuppressionCompte.Introuvable;
            }

            await transaction.CommitAsync();
            return ResultatSuppressionCompte.Supprime;
        }
        catch (Exception ex) when (EstViolationCleEtrangere(ex, CleEtrangerePossessionJeu))
        {
            return ResultatSuppressionCompte.Conflit;
        }
    }

    private static bool EstViolationCleEtrangere(Exception ex, string contrainte)
    {
        var violation = ex as PostgresException ?? ex.InnerException as PostgresException;

        return violation is { SqlState: PostgresErrorCodes.ForeignKeyViolation } && violation.ConstraintName == contrainte;
    }

    public async Task<IdentiteExterne?> TrouverIdentiteAsync(string fournisseur, string identifiant)
    {
        await using var contexte = await _fabrique.CreateDbContextAsync();

        return await contexte.IdentitesExternes
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Fournisseur == fournisseur &&
                                      i.IdentifiantCompteExterne == identifiant);
    }

    public async Task<Utilisateur?> TrouverUtilisateurAsync(Guid identifiant)
    {
        await using var contexte = await _fabrique.CreateDbContextAsync();

        return await contexte.Utilisateurs
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == identifiant);
    }

    public async Task<bool> VerifierExistenceCodeAmitieAsync(string code)
    {
        await using var contexte = await _fabrique.CreateDbContextAsync();

        return await contexte.Utilisateurs
            .AsNoTracking()
            .AnyAsync(u => u.CodeAmitie == code);
    }

    public async Task<bool> VerifierExistenceCourrielAsync(string courriel)
    {
        await using var contexte = await _fabrique.CreateDbContextAsync();

        if (await contexte.Utilisateurs.AsNoTracking().AnyAsync(u => u.Courriel == courriel))
        {
            return true;
        }

        return await contexte.IdentitesExternes.AsNoTracking().AnyAsync(i => i.Courriel == courriel);
    }
}
